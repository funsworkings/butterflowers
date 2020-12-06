using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = uwu.IO.SimpleFileBrowser.Scripts.FileSystemEntry;
using Memory = Wizard.Memory;
using System.Runtime.InteropServices;
using AI.Agent;
using Data;
using Objects.Base;
using Objects.Managers;
using Objects.Types;
using UnityEngine.Events;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using uwu;
using uwu.Camera;
using uwu.Extensions;
using uwu.Gameplay;
using uwu.IO;
using uwu.Snippets;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;


/*
 *
 *  Overhauled the pickup+drop sys
 *  Worked through polish notes on black hole with east
 *  
 *
 * 
 */



public class World : MonoBehaviour, ISaveable
{
    public static World Instance = null;
    public static bool LOAD = false;

    // Events

    public UnityEvent onDiscovery;

    // External

    [SerializeField] Settings.WorldPreset Preset;
    [SerializeField] CameraManager CameraManager;
    [SerializeField] Focusing Focusing;

    GameDataSaveSystem _Save = null;
    Library Library = null;
    FileNavigator Files = null;
    Surveillance Surveillance = null;

    ButterflowerManager Butterflowers = null;
    BeaconManager Beacons = null;
    VineManager Vines = null;
    EventManager EventsM = null;
    RemoteManager Remote = null;

    Sun Sun = null;
    Nest Nest = null;
    Quilt Quilt = null;
    Cage Cage = null;
    Brain AI = null;

    [SerializeField] Wand wand;

    // Attributes

    public string username;

    // Properties

    [SerializeField] Camera m_playerCamera = null;
    [SerializeField] Loading Loader = null;

    // Collections
    
    [Header("Entities")] 
        [SerializeField] List<Manager> managers = new List<Manager>();
        [SerializeField] List<Entity> entities = new List<Entity>();
        [SerializeField] List<Interactable> interactables = new List<Interactable>();
        [SerializeField] List<Focusable> focusables = new List<Focusable>();

    #region Accessors

    public Camera PlayerCamera => m_playerCamera;

    public WorldState state => Remote._State;
    public bool IsUser => (Remote._State == WorldState.User);
    public bool IsRemote => (Remote._State == WorldState.Remote);
    public bool IsParallel => (Remote._State == WorldState.Parallel);

    public bool IsFocused => Focusing.active;

    public Manager[] Managers => managers.ToArray();
    public Entity[] Entities => entities.ToArray();
    public Interactable[] Interactables => interactables.ToArray();
    public Focusable[] Focusables => focusables.ToArray();

    #endregion

    #region Monobehaviour callbacks

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        Sun = Sun.Instance;
        Sun.active = false; // Initialize sun to inactive on start

        _Save = GameDataSaveSystem.Instance;

        _Save.LoadGameData<SceneData>(createIfEmpty: true);
        _Save.LoadGameData<BrainData>("brain.fns", createIfEmpty: true);

        while (!_Save.load)
            yield return null;

        Surveillance = Surveillance.Instance;
        Library = Library.Instance;
        Files = FileNavigator.Instance;
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Butterflowers = FindObjectOfType<ButterflowerManager>();
        Beacons = FindObjectOfType<BeaconManager>();
        Vines = FindObjectOfType<VineManager>();
        Remote = FindObjectOfType<RemoteManager>();
        Cage = FindObjectOfType<Cage>();
        EventsM = FindObjectOfType<EventManager>();
        AI = FindObjectOfType<Brain>();

        /* * * * * * * * * * * * * * * * */

        username = _Save.username;
        Remote.Load(_Save.data.GAMESTATE); // Load game state

        /* * * * * * * * * * * * * * * * */


        SubscribeToEvents(); // Add all event listeners

        StartCoroutine("Initialize");
    }

    void OnDestroy()
    {
        UnsubscribeToEvents();
    }

    #endregion

    #region Initialization

    IEnumerator Initialize()
    {
        yield return new WaitForEndOfFrame(); // Padding at start of initialization -> SAFE

        Dictionary<string, Texture2D[]> texturePacks = new Dictionary<string, Texture2D[]>() 
        {
            {"WIZARD", Preset.wizardFiles.items.Select(mem => mem.image).ToArray()},
            {"ENVIRONMENT", Preset.starterFiles.elements}
        };

        Surveillance.Load(_Save.data.surveillanceData); // Load surveillance data
        
        
        var lib_payload = new LibraryPayload();
            lib_payload.directories = _Save.directories;
            lib_payload.files = _Save.files;
            lib_payload.userFiles = _Save.user_files;
            lib_payload.sharedFiles = _Save.shared_files;
            lib_payload.worldFiles = _Save.world_files;
            
        Library.Load(lib_payload, Preset.defaultNullTexture, texturePacks, Preset.loadTexturesInEditor);

        while (Library.loadProgress < 1f) 
        {
            Loader.progress = Library.loadProgress;
            yield return null;
        }

        Loader.progress = 1f;

        EventsM.Load(null);
        Nest.Load(_Save.nestOpen);
        AI.Load(new BrainPayload(_Save.brainData, _Save.enviro_knowledge, _Save.file_knowledge));
        Beacons.Load((Preset.persistBeacons) ? _Save.beaconData : null);
        Vines.Load((Preset.persistVines) ? _Save.data.vines : null);
        Sun.Load(_Save.data.sun);
        
        LOAD = true;
    }

    void SubscribeToEvents()
    {
        Surveillance.onCaptureLog += DidCaptureLog;
        Beacons.onUpdateBeacons += DidUpdateBeacons;
        Library.onDiscoverFile += DidDiscoverFile;
        Library.OnRefreshItems += DidUpdateLibraryItems;
        Remote.onParallel += DidBecomeParallel;
    }

    void UnsubscribeToEvents()
    {
        Surveillance.onCaptureLog -= DidCaptureLog;
        Beacons.onUpdateBeacons -= DidUpdateBeacons;
        Library.onDiscoverFile -= DidDiscoverFile;
        Library.OnRefreshItems -= DidUpdateLibraryItems;
        Remote.onParallel -= DidBecomeParallel;
    }

    #endregion

    #region Entities

    public void RegisterEntity(Element el)
    {
        if (el is Entity) 
        {
            var entity = (el as Entity);
            if (!entities.Contains(entity)) 
            {
                entities.Add(entity);

                if (entity is Interactable)
                    interactables.Add(entity as Interactable);
                if (entity is Focusable)
                    focusables.Add(entity as Focusable);
            }
        }
        else if (el is Manager) {
            var manager = (el as Manager);
            if(!managers.Contains(manager))
                managers.Add(manager);
        }
    }

    public void UnregisterEntity(Element el)
    {
        if (el is Entity) 
        {
            var entity = (el as Entity);
            if (entities.Contains(entity)) {
                entities.Remove(entity);

                if (entity is Interactable)
                    interactables.Remove(entity as Interactable);
                if (entity is Focusable)
                    focusables.Remove(entity as Focusable);
            }
        }
        else if (el is Manager) {
            var manager = (el as Manager);
            if (managers.Contains(manager))
                managers.Remove(manager);
        }
    }

    #endregion

    #region Beacons

    public Beacon.Status FetchBeaconStatus(Beacon beacon)
    {
        /*
        if (Preset.useWizard) 
        {
            var brain = Wizard.Brain;

            if (brain.isUnknown(beacon))
                return Beacon.Status.UNKNOWN;
            else if (brain.isComfortable(beacon))
                return Beacon.Status.COMFORTABLE;
            else if (brain.isActionable(beacon))
                return Beacon.Status.ACTIONABLE;
        }*/

        return Beacon.Status.NULL;
    }

    public float FetchBeaconKnowledgeMagnitude(Beacon beacon)
    {
        return 1f;
    }

    #endregion

    #region Suggestions

    public SUGGESTION[] GetSuggestions()
    {
        SUGGESTION suggestion = SUGGESTION.NULL;
        float max = -1f;

        suggestion = SUGGESTION.ENDOW_KINDNESS;
        max = (1f - Butterflowers.GetHealth());

        float nest = (1f - Nest.fill);
        if (nest > max) {
            suggestion = SUGGESTION.ADD_BEACON;
            max = nest;
        }

        /*float absorb = (1f - FetchKnowledgeOfWizard());
        if (absorb > max) {
            suggestion = SUGGESTION.VISIT_TREE;
            max = nest;
        }*/

        suggestion = SUGGESTION.VISIT_TREE;

        return new SUGGESTION[] {suggestion};
    }

    #endregion
    
    #region Entity callbacks

    void DidUpdateLibraryItems()
    {
        var payload = (LibraryPayload) Library.Save();
            _Save.directories = payload.directories;
            _Save.files = payload.files;
            _Save.user_files = payload.userFiles;
            _Save.shared_files = payload.sharedFiles;
            _Save.world_files = payload.worldFiles;
    }

    void DidCaptureLog()
    {
        _Save.data.surveillanceData = (SurveillanceData[])Surveillance.Save();
    }

    void DidUpdateBeacons()
    {
        _Save.beacons = (Beacon[]) Beacons.Save();
    }
    
    void DidBecomeParallel()
    {
        AI.Reload(); // Construct behaviour profile

        _Save.brainData.behaviourProfile = AI.Profile; // Assign profile
        _Save.brainData.load = true;
    }
    
    void DidDiscoverFile(string file)
    {
        Debug.LogFormat("Discovery was made => {0}", file);
        
        Events.ReceiveEvent(EVENTCODE.DISCOVERY, AGENT.User, AGENT.Beacon, details: file);
        onDiscovery.Invoke();
    }

    #endregion

    #region Save/load

    public object Save()
    {
        return -1;
    }

    public void Load(object data)
    {
        print("Load world data!");
    }

    #endregion
}
