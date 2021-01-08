using System.Collections;
using System.Collections.Generic;
using Data;
using Interfaces;
using Neue.Agent.Brain.Data;
using Objects.Base;
using Objects.Managers;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Camera;
using uwu.IO;
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



namespace Core
{
    public class World : MonoBehaviour, ISaveable, IReactToSunCycle, IPauseSun
    {
        public static World Instance = null;
        public static bool LOAD = false;

        // Events

        public UnityEvent onDiscovery;

        // External

        [Header("General")]
        [SerializeField] Settings.WorldPreset Preset;
        [SerializeField] CameraManager CameraManager;
        [SerializeField] Focusing Focusing;

        GameDataSaveSystem _Save = null;
        Library Library = null;
        FileNavigator Files = null;
        Surveillance Surveillance = null;

        [Header("Managers")]
        [SerializeField] ButterflowerManager Butterflowers = null;
        [SerializeField] BeaconManager Beacons = null;
        [SerializeField] VineManager Vines = null;
        [SerializeField] EventManager EventsM = null;
        [SerializeField] SummaryManager Summary = null;
        [SerializeField] SequenceManager Sequence = null;

        [Header("Objects")]
        Sun Sun = null;
        Nest Nest = null;
        Quilt Quilt = null;
        [SerializeField] Cage Cage = null;

        [SerializeField] Wand wand;

        // Attributes

        public string username;

        // Properties

        [SerializeField] Camera m_playerCamera = null;
        [SerializeField] Loading Loader = null;

        [SerializeField] ToggleOpacity gamePanel;
        [SerializeField] Profile profile;

        [SerializeField] bool wait = false;

        // Collections
    
        [Header("Entities")] 
        [SerializeField] List<Manager> managers = new List<Manager>();
        [SerializeField] List<Entity> entities = new List<Entity>();
        [SerializeField] List<Interactable> interactables = new List<Interactable>();
        [SerializeField] List<Focusable> focusables = new List<Focusable>();

        #region Accessors

        public Camera PlayerCamera => m_playerCamera;

        public bool Pause => wait;
        public bool IsFocused => Focusing.active;

        public Manager[] Managers => managers.ToArray();
        public Entity[] Entities => entities.ToArray();
        public Interactable[] Interactables => interactables.ToArray();
        public Focusable[] Focusables => focusables.ToArray();

        public Profile Profile => profile;

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

            _Save.LoadGameData<GameData>(createIfEmpty: true);

            while (!_Save.load)
                yield return null;

            Surveillance = Surveillance.Instance;
            Library = Library.Instance;
            Files = FileNavigator.Instance;
            Nest = Nest.Instance;
            Quilt = Quilt.Instance;

            /* * * * * * * * * * * * * * * * */

            username = _Save.username;

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
                //{"WIZARD", Preset.wizardFiles.items.Select(mem => mem.image).ToArray()},
                //{"ENVIRONMENT", Preset.starterFiles.elements}
                { "WIZARD", Preset.defaultTextures }
            };

            Surveillance.Load(_Save.data.surveillanceData); // Load surveillance data

            var lib_payload = new LibraryPayload();
            lib_payload.directories = _Save.data.directories;
            lib_payload.files = _Save.data.files;
            lib_payload.userFiles = _Save.data.user_files;
            lib_payload.sharedFiles = _Save.data.shared_files;
            lib_payload.worldFiles = _Save.data.world_files;
            
            Library.Load(lib_payload, Preset.defaultNullTexture, texturePacks, Preset.loadTexturesInEditor);
            Butterflowers.Load();
            
            yield return new WaitForEndOfFrame();
            
            Loader.Load();
            while (Loader.IsLoading) yield return null;

            EventsM.Load(null);
            Sequence.Load(_Save.data.sequence);
            Nest.Load(_Save.data.nestopen);
            Beacons.Load((Preset.persistBeacons) ? _Save.data.beacons : null);
            Vines.Load((Preset.persistVines) ? _Save.data.vines : null);
            Sun.Load(_Save.data.sun);
        
            LOAD = true;
        }

        public void Cycle(bool refresh)
        {
            profile = Surveillance.ConstructBehaviourProfile(); // Create new behaviour profile

            wait = true;
            StartCoroutine(Advance());
        }
    
        IEnumerator Advance()
        {
            Sun.active = false;
            yield return new WaitForEndOfFrame();

            while (Surveillance.inprogress) yield return null;
            
            SaveLibraryItems();
            _Save.data.sun = (SunData) Sun.Save();
            _Save.data.surveillanceData = (SurveillanceData[])Surveillance.Save();
            _Save.data.sequence = (SequenceData) Sequence.Save();
            _Save.data.nestopen = (bool) Nest.Save();
            _Save.data.beacons = (BeaconSceneData) Beacons.Save();
            _Save.data.vines = (VineSceneData) Vines.Save();

            _Save.SaveGameData(); // Save all game data
            yield return new WaitForEndOfFrame();
            
            gamePanel.Hide();
            
            Summary.ShowSummary();
            while (Summary.Pause) yield return null;
            
            Sequence.Cycle();
            while (Sequence.Pause) yield return null;

            wait = false;
            gamePanel.Show();
        }

        void SubscribeToEvents()
        {
            Library.onDiscoverFile += DidDiscoverFile;
        }

        void UnsubscribeToEvents()
        {
            Library.onDiscoverFile -= DidDiscoverFile;
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
        
        void SaveLibraryItems()
        {
            var payload = (LibraryPayload) Library.Save();
                _Save.data.directories = payload.directories;
                _Save.data.files = payload.files;
                _Save.data.user_files = payload.userFiles;
                _Save.data.shared_files = payload.sharedFiles;
                _Save.data.world_files = payload.worldFiles;
        }

        public void Load(object data)
        {
            print("Load world data!");
        }

        #endregion
    }
}
