using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = uwu.IO.SimpleFileBrowser.Scripts.FileSystemEntry;
using Memory = Wizard.Memory;
using System.Runtime.InteropServices;
using AI.Agent;
using Objects.Managers;
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

public class World : MonoBehaviour
{
    public static World Instance = null;
    public static bool LOAD = false;

    // Events

    public UnityEvent onDiscovery;

    public static System.Action<State> onUpdateState;
    
    #region Internal

    public enum State
    {
        User = 0,
        Remote = 1,
        Parallel = 2
    }
    
    #endregion

    // External

	[SerializeField] Settings.WorldPreset Preset;
    [SerializeField] CameraManager CameraManager;
    [SerializeField] Focusing Focusing;

    GameDataSaveSystem Save = null;
    Library Library = null;
    FileNavigator Files = null;
    Surveillance Surveillance = null;

    ButterflowerManager Butterflowers = null;
    BeaconManager Beacons = null;
    VineManager Vines = null;
    EventManager EventsM = null;

    Sun Sun = null;
    Nest Nest = null;
    Quilt Quilt = null;
    Cage Cage = null;
    Brain AI = null;

    [SerializeField] Wand wand;

    // Attributes

    public State state = State.User;
    public string username;

    float remoteTimer = 0f;

    // Properties
    
    [SerializeField] Camera m_playerCamera = null;

    [SerializeField] Loading Loader = null;
    [SerializeField] Loading Reloader = null;
    [SerializeField] ToggleOpacity ReloadContainer;

    [SerializeField] UnityEngine.UI.Image remoteProgressBar;
    [SerializeField] ToggleOpacity userOverlay, remoteOverlay;

    // Collections
    
    [Header("Entities")]
    
    [SerializeField] List<Entity> entities = new List<Entity>();
    [SerializeField] List<Interactable> interactables = new List<Interactable>();
    [SerializeField] List<Focusable> focusables = new List<Focusable>();

    #region Accessors
    
    public Camera PlayerCamera => m_playerCamera;

    public bool User => (state == State.User);
    public bool Remote => (state == State.Remote);
    public bool Parallel => (state == State.Parallel);

    public bool IsFocused => Focusing.active;

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
        
        Save = GameDataSaveSystem.Instance;
        
        Save.LoadGameData<SceneData>(createIfEmpty: true);
        Save.LoadGameData<BrainData>("brain.fns", createIfEmpty:true);
        
        while (!Save.load) 
            yield return null;

        username = Save.username;
        state = (Save.data.GAMESTATE != State.Parallel) ? State.User : State.Parallel;

        Surveillance = Surveillance.Instance;
        
        Library = Library.Instance;

        Files = FileNavigator.Instance;
        
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Butterflowers = FindObjectOfType<ButterflowerManager>();
        Beacons = FindObjectOfType<BeaconManager>();
        Vines = FindObjectOfType<VineManager>();
        Cage = FindObjectOfType<Cage>();
        EventsM = FindObjectOfType<EventManager>();
        AI = FindObjectOfType<Brain>();
        
        SubscribeToEvents(); // Add all event listeners

        StartCoroutine("Initialize");
    }

    void Update()
    {
        if (!Save.load) return;
        Save.data.GAMESTATE = state;

        State newState = UpdateRemoteStatus();

        if (state != newState) {
            if (onUpdateState != null)
                onUpdateState(newState);

            state = newState;
        }
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
            { "WIZARD" , Preset.wizardFiles.items.Select(mem => mem.image).ToArray() },
            { "ENVIRONMENT", Preset.starterFiles.elements }
        };
        
        Surveillance.Load(Save.data.surveillanceData); // Load surveillance data
        Library.Load(Save.files, Preset.defaultNullTexture, texturePacks, Preset.loadTexturesInEditor);

        while (Library.loadProgress < 1f) 
        {
            Loader.progress = Library.loadProgress;
            yield return null;
        }
        Loader.progress = 1f;
        
        EventsM.Load(null);
        
        if (Save.nestOpen)
            Nest.Open();
        else
            Nest.Close();

        AI.Load();

        Beacons.Load();
        if (Preset.persistBeacons)
            Beacons.RestoreBeacons(Save.beaconData, deprecate: false);
        else
            Beacons.RefreshBeacons();

        if (Preset.persistVines)
            Vines.Load(Save.vineData);
        else
            Vines.Load(null);

        Sun.active = true; // Update sun state
        LOAD = true;
    }

    void SubscribeToEvents()
    {
        Library.onDiscoverFile += onDiscoverFile;
        Focusing.onUpdateState += onUpdateFocusState;

        Events.onGlobalEvent += onGlobalEvent;

        Nest.onOpen.AddListener(onNestStateChanged);
        Nest.onClose.AddListener(onNestStateChanged);
        Nest.onAddBeacon += onIngestBeacon;
        Nest.onRemoveBeacon += onReleaseBeacon;

        Quilt.onDisposeTexture += onDisposeQuiltTexture;

        Cage.onCompleteCorners += DetachSelf;
    }

    void UnsubscribeToEvents()
    {
        Library.onDiscoverFile -= onDiscoverFile;

        Focusing.onUpdateState -= onUpdateFocusState;

        Events.onGlobalEvent -= onGlobalEvent;

        Nest.onOpen.RemoveListener(onNestStateChanged);
        Nest.onClose.RemoveListener(onNestStateChanged);
        Nest.onAddBeacon -= onIngestBeacon;
        Nest.onRemoveBeacon -= onReleaseBeacon;

        Quilt.onDisposeTexture -= onDisposeQuiltTexture;
        
        Cage.onCompleteCorners -= DetachSelf;
    }

    #endregion
    
    #region Entities

    public void RegisterEntity(Entity entity)
    {
        if (!entities.Contains(entity)) {
            entities.Add(entity);
            
            if(entity is Interactable)
                interactables.Add(entity as Interactable);
            if(entity is Focusable)
                focusables.Add(entity as Focusable);
        }
    }

    public void UnregisterEntity(Entity entity)
    {
        if (entities.Contains(entity)) {
            entities.Remove(entity);
            
            if(entity is Interactable)
                interactables.Remove(entity as Interactable);
            if(entity is Focusable)
                focusables.Remove(entity as Focusable);
        }
    }

    #endregion

    #region Remote access

    State UpdateRemoteStatus()
    {
        State targetState = State.User;

        if (state == State.Parallel) 
        {
            remoteTimer = 0f;
            targetState = State.Parallel;
            UpdateUserInterfaces(-1f);
        }
        else 
        {
            if (!wand.spells || wand.speed2d < Preset.cursorSpeedThreshold) 
            {
                remoteTimer += Time.deltaTime;

                float remoteInterval =
                    Mathf.Clamp01((remoteTimer - Preset.cursorIdleDelay) / Preset.cursorIdleTimeThreshold);
                targetState = (remoteInterval >= 1f) ? State.Remote : State.User;

                UpdateUserInterfaces(remoteInterval);
            }
            else {
                remoteTimer = 0f;
                targetState = State.User;

                UpdateUserInterfaces(0f);
            }
        }

        return targetState;
    }

    void UpdateUserInterfaces(float progress)
    {
        if (!User) 
        {
            remoteOverlay.Show();

            if (progress < 0f)
                userOverlay.Show();
            else
                userOverlay.Hide();
        }
        else {
            remoteOverlay.Hide();
            userOverlay.Show();
        }

        remoteProgressBar.fillAmount = (!User) ? 0f : progress;
    }

    void DetachSelf()
    {
        bool @new = (state != State.Parallel);

        if (@new) 
        {
            AI.Reload(); // Construct behaviour profile
            
            Save.brainData.behaviourProfile = AI.Profile; // Assign profile
            Save.brainData.load = true;
        }
        
        state = State.Parallel; // Initialize parallel AI
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

        return new SUGGESTION[] { suggestion };
    }

    #endregion

    #region Focus callbacks

    void onUpdateFocusState(Focusing.State previous, Focusing.State current)
    {
        Debug.LogFormat("Focus state changed from {0} to {1}", previous, current);
    }

    #endregion

    #region Event callbacks

    void onGlobalEvent(EVENTCODE @event)
    {
        
    }

	#endregion

    #region Nest callbacks

    void onNestStateChanged()
    {
        Debug.LogFormat("Nest state changed to --> {0}", Nest.open);
        Save.nestOpen = Nest.open; // Set save state in file
    }

    void onIngestBeacon(Beacon beacon)
    {
        Save.beacons = Beacons.AllBeacons;

        if (beacon.type == Beacon.Type.None) return;

        var file = beacon.file;
        var tex = Library.GetTexture(file);

        if (tex != null) {
            Debug.Log("Load in LIBRARY");
            Quilt.Add(tex); // Load texture from library
        }
        else {
            Debug.Log("Load from LIBRARY");
            Quilt.Push(file); // Load texture in quilt
        }
    }

    void onReleaseBeacon(Beacon beacon)
    {
        Save.beacons = Beacons.AllBeacons.ToArray();

        if (beacon.type == Beacon.Type.None) return;

        var file = beacon.file;
        Quilt.Pop(file);
    }

    #endregion

    #region Quilt callbacks

    void onDisposeQuiltTexture(Texture texture)
    {

    }

    #endregion

    #region Discovery callbacks

    void onDiscoverFile(string file)
    {
        Debug.LogFormat("Discovery was made => {0}", file);
        
        onDiscovery.Invoke();
        Events.ReceiveEvent(EVENTCODE.DISCOVERY, AGENT.User, AGENT.Beacon, details:file);
    }

    #endregion
}
