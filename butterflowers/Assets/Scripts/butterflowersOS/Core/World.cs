using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using butterflowersOS.Data;
using butterflowersOS.Interfaces;
using butterflowersOS.Menu;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using Neue.Agent.Brain.Data;
using Objects.Managers;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Camera;
using uwu.Data;
using uwu.IO;
using uwu.Snippets.Load;
using uwu.Timeline.Core;
using uwu.UI.Behaviors.Visibility;


/*
 *
 *  Overhauled the pickup+drop sys
 *  Worked through polish notes on black hole with east
 *  
 *
 * 
 */



namespace butterflowersOS.Core
{
    public class World : MonoBehaviour, ISaveable, IReactToSunCycleReliable, IPauseSun
    {
        public static World Instance = null;
        public static bool LOAD = false;
        
        
        #region Internal

        public enum AdvanceType
        {
            Broken,
            Continuous
        }
        
        #endregion

        // Events

        public UnityEvent onDiscovery;

        // External

        [Header("General")]
        [SerializeField] WorldPreset Preset;
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
        [SerializeField] Cutscenes Cutscenes = null;

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
        [SerializeField] Loader Loader = null;

        [SerializeField] ToggleOpacity gamePanel;
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] Profile profile;

        [SerializeField] bool wait = false;
        [SerializeField] bool dispose = false;

        private float m_TimeScale = 1f;
        public float TimeScale
        {
            get { return m_TimeScale; }
            private set { m_TimeScale = value; }
        }

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
            Loader = Loader.Instance;

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
            type = (_Save.data.profileGenerated && _Save.IsProfileValid()) ? AdvanceType.Continuous : AdvanceType.Broken; // Adjust type from loaded profile!
            
            if(type == AdvanceType.Continuous)
                profile = _Save.data.brain.profile;
            else
                profile = _Save.data.profile;

            /* * * * * * * * * * * * * * * * */

            SubscribeToEvents(); // Add all event listeners

            StartCoroutine("Initialize");
        }

        void Update()
        {
            if (dispose) return;
            UpdateTimeScale();
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
            
            Surveillance.Load(_Save.data.surveillanceData); // Load surveillance data

            
            Dictionary<string, Texture2D[]> texturePacks = new Dictionary<string, Texture2D[]>() 
            {
                { "DEFAULT", Preset.defaultTextures }
            };
            LoadLibraryItems(texturePacks);
            
            Butterflowers.Load();
            
            yield return new WaitForEndOfFrame();
            
            Loader.Load(.33f, 1f);
            while (Loader.IsLoading) yield return null;
            Loader.Dispose();

            EventsM.Load(null);
            Sequence.Load(_Save.data.sequence);
            Nest.Load(_Save.data.nestopen);
            Beacons.Load((Preset.persistBeacons) ? _Save.data.beacons : null);
            Vines.Load((Preset.persistVines) ? _Save.data.vines : null);
            Sun.Load(_Save.data.sun);

            yield return new WaitForEndOfFrame();

            Surveillance.New(onload: true); // Trigger surveillance (if profile not generated!)
           
            LOAD = true;
        }

        public void Cycle(bool refresh)
        {
            wait = true;
            StartCoroutine(Advance());
        }

        public AdvanceType type = AdvanceType.Continuous;
        
        IEnumerator Advance()
        {
            AdvanceType _type = (_Save.data.profileGenerated && _Save.IsProfileValid()) ? AdvanceType.Continuous : AdvanceType.Broken; // Adjust type from loaded profile!
            
            if (_type == AdvanceType.Broken)  // Deactivate sun
            {
                Sun.active = false;
                yield return new WaitForEndOfFrame();
            }
            
            Surveillance.Stop();
            Surveillance.Dispose();
            while (Surveillance.recording) yield return null;

            SaveLibraryItems();
            _Save.data.sun = (SunData) Sun.Save();
            _Save.data.sequence = (SequenceData) Sequence.Save();
            _Save.data.nestopen = (bool) Nest.Save();
            _Save.data.beacons = (BeaconSceneData) Beacons.Save();
            _Save.data.vines = (VineSceneData) Vines.Save();

            if (_type == AdvanceType.Broken) 
            {
                profile = Surveillance.ConstructBehaviourProfile(); // Generate profile cached
                
                _Save.data.profile = profile; // Assign generated profile
                _Save.data.surveillanceData = (SurveillanceData[])Surveillance.Save(); // Continue saving surveillance data if profile has not been generated
            }
            else 
            {
                profile = _Save.data.brain.profile; // Load existing profile
            }

            _Save.SaveGameData(); // Save all game data
            yield return new WaitForEndOfFrame();

            SequenceManager.TriggerReason sequenceReason = SequenceManager.TriggerReason.Block;
            bool didLoadSequence = false;
            
            if (_type == AdvanceType.Broken)  // Wait for summary and sequence items
            {
                gamePanel.Hide();

                Summary.ShowSummary(profile);
                while (Summary.Pause) yield return null;

                sequenceReason = Sequence.Cycle();

                didLoadSequence = (sequenceReason == SequenceManager.TriggerReason.Nothing);
                if (didLoadSequence) 
                {
                    while (Sequence.Pause) yield return null;
                    Nest.RandomKick(); // Re-activate nest after sequence pause
                }
            }
            
            Surveillance.New(); // Trigger
            Beacons.RefreshBeacons(); // Refresh all beacons

            wait = false; // Disable pause!
            type = _type;
            
            if (didLoadSequence) 
            {
                while (Sequence.Read)
                    yield return null; // Wait for sequence captions to finish before showing game UI panel
            }

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
        
        #region Time scale

        void UpdateTimeScale()
        {
            if (pauseMenu.IsVisible) TimeScale = 0f;
            else TimeScale = 1f;
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
        
        #region Neueagent

        [ContextMenu("Generate neue agent")]
        public void ExportNeueAgent()
        {
            if (Pause) return; // Ignore request to export if paused
            
            string path = GetExportPath();
            BrainData data = new BrainData(_Save.data);
            
            bool success = ExportProfile(path, data);
            Debug.LogWarningFormat("{0} generating profile => {1}", (success)? "Success":"Fail", path);
            
            if (success) 
            {
                _Save.data.brain = data;
                _Save.data.profileGenerated = _Save.IsProfileValid();
                _Save.data.profile = profile = data.profile;
            }
            else 
            {
                _Save.data.brain = default(BrainData);
                _Save.data.profileGenerated = false;
                _Save.data.profile = profile = Surveillance.ConstructBehaviourProfile();
            }

            type = (_Save.data.profileGenerated) ? AdvanceType.Continuous : AdvanceType.Broken; // Adjust type from generated profile
            _Save.SaveGameData(); // Save all data
        }

        public void ImportNeueAgent(BrainData brainData)
        {
            if (Pause) return; // Ignore request to import if paused
        
            _Save.data.brain = brainData;
            _Save.data.profileGenerated = true;
            _Save.data.profile = profile = brainData.profile;
            
            type = AdvanceType.Continuous;
            
            //AggregateBrainFiles(brainData);
            
            Debug.LogWarning("Successfully imported brain profile for user => " + brainData.username);
            
            _Save.SaveGameData();
        }

        [ContextMenu("Wipe neue agent")]
        public void WipeNeueAgent()
        {
            if (Pause) return; // Ignore request to wipe if paused
        
            string username = (_Save.data.brain.username);
            
            _Save.data.brain = null;
            _Save.data.profileGenerated = false;
            _Save.data.profile = profile = Surveillance.ConstructBehaviourProfile();
            
            type = AdvanceType.Broken;

            Debug.LogWarning("Successfully wiped brain profile for user => " + (string.IsNullOrEmpty(username)? "NULL":username));

            _Save.SaveGameData();
        }

        
        
        string GetExportPath()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            desktop = Application.persistentDataPath; // Use local data path


            var username = _Save.data.username;
            
            Regex reg = new Regex("[*'\",_&#^@]");
            username = reg.Replace(username, "_"); // Replace all special characters in exported neue
            
            var path = Path.Combine(desktop, username + ".fns");
            return path;
        }
        
        bool ExportProfile(string path, BrainData data)
        { 
            bool success = DataHandler.Write(data, path); // Write
            return success;
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

        void AggregateBrainFiles(BrainData data)
        {
            if (data.IsProfileValid()) 
            {
                var lib_payload = new LibraryPayload();
                lib_payload.directories = data.directories;
                lib_payload.files = data.files;
                lib_payload.userFiles = data.user_files;
                lib_payload.sharedFiles = data.shared_files;
                lib_payload.worldFiles = data.world_files;
                
                Library.Aggregate(lib_payload);
            }
        }

        void LoadLibraryItems(Dictionary<string, Texture2D[]> texturePacks)
        {
            var lib_payload = new LibraryPayload();
            lib_payload.directories = _Save.data.directories;
            lib_payload.files = _Save.data.files;
            lib_payload.userFiles = _Save.data.user_files;
            lib_payload.sharedFiles = _Save.data.shared_files;
            lib_payload.worldFiles = _Save.data.world_files;
            
            Library.Load(lib_payload, Preset.defaultNullTexture, texturePacks, Preset.loadTexturesInEditor, Preset.backlogTextures);
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
