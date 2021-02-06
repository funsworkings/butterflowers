using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using B83.Win32;
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
using UnityEngine.Playables;
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
    public class World : Importer, ISaveable, IReactToSunCycleReliable, IPauseSun
    {
        public static World Instance = null;
        public static bool LOAD = false;
        
        
        #region Internal

        public enum AdvanceType
        {
            Broken,
            Continuous
        }

        public enum State
        {
            Load,
            
            Cutscene,
            Summary,
            Game
        }
        
        #endregion

        // Events

        public UnityEvent onDiscovery;
        public UnityEvent onLoad, onLoadComplete, onLoadTrigger;

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
        [SerializeField] CutsceneManager Cutscenes = null;
        [SerializeField] NotificationCenter NotificationCenter = null;
        [SerializeField] TutorialManager Tutorial = null;

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
        [SerializeField] SceneAudioManager sceneAudio;
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] Profile profile;

        [SerializeField] bool wait = false;
        [SerializeField] bool dispose = false;


        byte[] IMAGES = new byte[] { };
        int IMAGE_ROWS = 0, IMAGE_COLUMNS = 0;
        

        private float m_TimeScale = 1f;
        public float TimeScale
        {
            get { return m_TimeScale; }
            private set { m_TimeScale = value; }
        }

        public bool ready = true;
        
        public State _State { get; set; }

        // Collections
    
        [Header("Entities")] 
            [SerializeField] List<Manager> managers = new List<Manager>();
            [SerializeField] List<Entity> entities = new List<Entity>();
            [SerializeField] List<Interactable> interactables = new List<Interactable>();
            [SerializeField] List<Focusable> focusables = new List<Focusable>();

        [Header("Audio")] 
            [SerializeField] AudioSource loadAudio;
            [SerializeField] AudioClip loadAudioClip;
            
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
            type = (_Save.IsSelfProfileValid()) ? AdvanceType.Continuous : AdvanceType.Broken; // Adjust type from loaded profile!
            
            profile = _Save.data.profile;

            /* * * * * * * * * * * * * * * * */

            SubscribeToEvents(); // Add all event listeners

            gamePanel.Hide();
            pauseMenu.enabled = false;
            
            StartCoroutine("Initialize");
        }

        void Update()
        {
            if (dispose) return;
            
            HandleReady();
            UpdateTimeScale();
        }
        
        void HandleReady()
        {
            ready = (!pauseMenu.IsVisible && !Cutscenes.inprogress); // Wait for pause menu and cutscenes!
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
            
            sceneAudio.FadeIn();
            yield return new WaitForEndOfFrame();

            onLoad.Invoke();
            Loader.Load(.1f, 1f);
            while (Loader.IsLoading) yield return null;
            
            onLoadComplete.Invoke();
            while (loadAudio.isPlaying) yield return null;
            onLoadTrigger.Invoke();

            bool requireTutorial = !_Save.data.tutorial;
            if (requireTutorial && Tutorial.IsValid) {
                
                Tutorial.Begin();
                while (Tutorial.inprogress || Tutorial.dispose) yield return null;
                
                _Save.data.tutorial = true;
                _Save.SaveGameData(); // Save immediately when tutorial completed!
                
            }


            EventsM.Load(null);
            Cutscenes.Load(_Save.data.cutscenes);
            Sequence.Load(_Save.data.sequence);
            Nest.Load(false);
            Beacons.Load((Preset.persistBeacons) ? _Save.data.beacons : null);
            Vines.Load((Preset.persistVines) ? _Save.data.vines : null);
            Sun.Load(_Save.data.sun);

            yield return new WaitForEndOfFrame();

            if (type == AdvanceType.Broken)
                Surveillance.New(onload: true); // Trigger surveillance (if profile not generated!)
            else
                Surveillance.Ignore();


            if (!Cutscenes.intro) 
            {
                Cutscenes.TriggerIntro(); // Trigger intro cutscene    
                yield return new WaitForEndOfFrame();
            }
           
            LOAD = true;
            Loader.Dispose();
            
            while(Cutscenes.inprogress) yield return null; // Wait for cutscenes to wrap on open before showing game panel
            
            gamePanel.Show();
            pauseMenu.enabled = true;
        }

        public void Cycle(bool refresh)
        {
            wait = true;
            StartCoroutine(Advance());
        }

        public AdvanceType type = AdvanceType.Continuous;
        
        IEnumerator Advance()
        {
            AdvanceType _type = (_Save.IsSelfProfileValid()) ? AdvanceType.Continuous : AdvanceType.Broken; // Adjust type from loaded profile!
            
            if (_type == AdvanceType.Broken)  // Deactivate sun
            {
                Sun.active = false;
                yield return new WaitForEndOfFrame();
            }

            if (Surveillance.IsRecording) 
            {
                Surveillance.Stop();
                Surveillance.Dispose();
                
                while (Surveillance.recording) 
                    yield return null;
            }
            _Save.data.surveillanceData = (SurveillanceData[])Surveillance.Save(); // Continue saving surveillance data if profile has not been generated
            
            while(Cutscenes.inprogress) yield return null; // Wait for all cutscenes to dispose before continuing

            SaveLibraryItems();
            _Save.data.sun = (SunData) Sun.Save();
            _Save.data.sequence = (SequenceData) Sequence.Save();
            _Save.data.beacons = (BeaconSceneData) Beacons.Save();
            _Save.data.vines = (VineSceneData) Vines.Save();

            if (_type == AdvanceType.Broken) 
            {
                profile = Surveillance.ConstructBehaviourProfile(); // Generate profile cached
                _Save.data.profile = profile; // Assign generated profile
            }
            else 
            {
                profile = _Save.data.profile; // Load existing profile
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

                didLoadSequence = (sequenceReason == SequenceManager.TriggerReason.Success);
                if (didLoadSequence) 
                {
                    while (Sequence.Pause) yield return null;
                }
            }

            if (_type == AdvanceType.Broken)
                Surveillance.New(); // Trigger
            else
                Surveillance.Ignore();

            Beacons.RefreshBeacons(); // Refresh all beacons
            if (!string.IsNullOrEmpty(Surveillance.lastPhotoPath)) 
            {
                Beacons.CreateBeacon(Surveillance.lastPhotoPath, Beacon.Type.Desktop, Beacon.Locale.Terrain,
                    transition: BeaconManager.TransitionType.Spawn);
            }

            wait = false; // Disable pause!
            type = _type;
            
            
            if (!didLoadSequence) 
            {
                if (sequenceReason == SequenceManager.TriggerReason.SequenceHasCompleted && !_Save.data.export) // Has passed all sequence frames, begin export!
                {
                    Texture2D tex;
                    
                    IMAGES = Library.ExportSheet("test", out IMAGE_ROWS, out IMAGE_COLUMNS, out tex, oColumns:1);

                    if (!Cutscenes.outro) 
                    {
                        Cutscenes.TriggerOutro(IMAGE_ROWS, IMAGE_COLUMNS, tex);
                        while (!Cutscenes.inprogress) yield return null; // Wait for cutscene to finish
                    }
                    else 
                    {
                        ExportNeueAgent();
                    }
                }
            }

            while(Cutscenes.inprogress) yield return null;
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
            if (pauseMenu.IsVisible || pauseMenu.Dispose) TimeScale = 0f;
            else TimeScale = 1f;
        }
        
        #endregion

        #region Beacons

        public Beacon.Status FetchBeaconStatus(Beacon beacon)
        {
            /*
        if (Preset.useWizard) 
        {
            var brain = World.Brain;

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

        [ContextMenu("Export test agent")]
        public void ExportNeueAgent()
        {
            Texture2D tex;
                    
            IMAGES = Library.ExportSheet("test", out IMAGE_ROWS, out IMAGE_COLUMNS, out tex);
            ExportNeueAgent(IMAGES, (ushort)IMAGE_ROWS);
        }
        
        public void ExportNeueAgent(byte[] images, ushort image_height)
        {
            string file = "";
            string ext = "";
            
            string path = GetExportPath(out file, out ext);
            
            BrainData data = new BrainData(_Save.data, images, image_height);
            
            bool success = ExportProfile(path, data);
            Debug.LogWarningFormat("{0} generating profile => {1}", (success)? "Success":"Fail", path);
            
            if (success) 
            {
                _Save.data.export = true;
                _Save.data.export_agent_created_at = data.created_at;
                
                UploadToS3(string.Format("{0}_{1}" + ext, file, DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss")), path); // Upload generated neueagent to server :)
                
                NotificationCenter.TriggerExportNotif(path);
            }
            else 
            {
                _Save.data.export = false;
                _Save.data.export_agent_created_at = "";
            }
        }


        private string awsURLBaseVirtual = "";
        
        private const string awsBucketName = "neueagents";
        private const string awsAccessKey = "AKIAUZAW2DYXALSZUREP";
        private const string awsSecretKey = "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";

        public string DebugFilePath, DebugFileName;

        void UploadToS3(string FileName, string FilePath)
        {
            try {

                awsURLBaseVirtual = "https://" +
                                    awsBucketName +
                                    ".s3.amazonaws.com/";


                string currentAWS3Date =
                    System.DateTime.UtcNow.ToString(
                        "ddd, dd MMM yyyy HH:mm:ss ") +
                    "GMT";
                string canonicalString =
                    "PUT\n\n\n\nx-amz-date:" +
                    currentAWS3Date + "\n/" +
                    awsBucketName + "/" + FileName;
                UTF8Encoding encode = new UTF8Encoding();
                HMACSHA1 signature = new HMACSHA1();
                signature.Key = encode.GetBytes(awsSecretKey);
                byte[] bytes = encode.GetBytes(canonicalString);
                byte[] moreBytes = signature.ComputeHash(bytes);
                string encodedCanonical = Convert.ToBase64String(moreBytes);
                string aws3Header = "AWS " +
                                    awsAccessKey + ":" +
                                    encodedCanonical;
                string URL3 = awsURLBaseVirtual + FileName;
                WebRequest requestS3 =
                    (HttpWebRequest) WebRequest.Create(URL3);
                requestS3.Headers.Add("Authorization", aws3Header);
                requestS3.Headers.Add("x-amz-date", currentAWS3Date);
                byte[] fileRawBytes = File.ReadAllBytes(FilePath);
                requestS3.ContentLength = fileRawBytes.Length;
                requestS3.Method = "PUT";
                Stream S3Stream = requestS3.GetRequestStream();
                S3Stream.Write(fileRawBytes, 0, fileRawBytes.Length);
                /*Debug.Log("Sent bytes: " +
                          requestS3.ContentLength +
                          ", for file: " +
                          FileName);*/
                S3Stream.Close();

            }
            catch (System.Exception err) 
            {
                Debug.LogWarning("Unable to upload neueagent to server!");
            }
        }

        public string debugImportNeueagentPath = "";

        [ContextMenu("Import neueagent")]
        public void ImportNeueAgent()
        {
            BrainData dat = DataHandler.Read<BrainData>(debugImportNeueagentPath);
            if (dat != null) 
            {
                Debug.LogWarning("Validate => " + dat.created_at);
                
                if (dat.IsProfileValid()) 
                {
                   ImportNeueAgent(dat); // Break out of loop, successfully found file!
                }
            }
        }

        public void ImportNeueAgent(BrainData brainData)
        {
            if (Pause) return; // Ignore request to import if paused
            if (!_Save.IsSelfProfileValid()) return; // Ignore reques to import if not generated neueagent

            string @self = _Save.data.export_agent_created_at;
            string @agent = _Save.data.agent_created_at;
            string @other = brainData.created_at;
            
            if (@agent == @other || @self != @other) return; // Ignore request to import duplicate neueagent
        
            bool success = AggregateBrainData(brainData);
            type = (success && _Save.IsProfileValid()) ? AdvanceType.Continuous : AdvanceType.Broken;

            if (success) 
            {
                _Save.SaveGameData();
                StartCoroutine("MoveToNeueAgent");
            }
            
            Debug.LogWarning("Successfully imported brain profile for user => " + brainData.username);
        }

        IEnumerator MoveToNeueAgent()
        {
            yield return null;

            gamePanel.Hide();
            while(gamePanel.Visible) yield return null;
            
            SceneLoader.Instance.GoToScene(2, 0f, .1f); // Move to neue agent scene
        }

        bool AggregateBrainData(BrainData brainData)
        {
            bool success = false;
            
            var lib_payload = new LibraryPayload();
                lib_payload.directories = brainData.directories;
                lib_payload.files = brainData.files;
                lib_payload.userFiles = brainData.user_files.Select(uf => (int)uf).ToArray();
                lib_payload.sharedFiles = brainData.shared_files.Select(sf => (int)sf).ToArray();
                lib_payload.worldFiles = brainData.world_files.Select(wf => (int)wf).ToArray();

            if (Library.AggregateNeueAgentData(lib_payload)) 
            {
                success = Surveillance.AggregateNeueAgentData(brainData.surveillanceData);
            }

            if (success) 
            {
                _Save.data.surveillanceData = brainData.surveillanceData;
                
                _Save.data.agent_created_at = brainData.created_at;
                _Save.data.username = brainData.username;
                _Save.data.profile = profile = brainData.profile;
                _Save.data.images = brainData.images;
                _Save.data.image_height = brainData.image_height;

                _Save.data.agent_event_stack = brainData.surveillanceData.Length; // Total stack of events to parse from
            }

            return success;
        }

        [ContextMenu("Wipe neueagent")]
        public void WipeNeueAgent() // NEVER HAPPENS IN CURRENT ITERATION
        {
            if (Pause) return; // Ignore request to wipe if paused
            if (!_Save.IsProfileValid()) return;
        
            string username = (_Save.data.username);
            
            _Save.data.agent_created_at = "";
            _Save.data.username = PlayerPrefs.GetString("_Username"); // Get previous username
            _Save.data.profile = profile = Surveillance.ConstructBehaviourProfile();
            
            type = AdvanceType.Broken;
            Debug.LogWarning("Successfully wiped brain profile for user => " + (string.IsNullOrEmpty(username)? "NULL":username));
        }


        string GetExportPath(out string filename, out string extension)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //desktop = Application.persistentDataPath; // Use local data path
            
            var username = _Save.data.username;
            
            Regex reg = new Regex("[*'\",_&#^@]");
            username = reg.Replace(username, "_"); // Replace all special characters in exported neue

            filename = username;
            extension = ".fns";
            
            var path = Path.Combine(desktop, filename + extension);
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

        void LoadLibraryItems(Dictionary<string, Texture2D[]> texturePacks)
        {
            var lib_payload = new LibraryPayload();
            lib_payload.directories = _Save.data.directories;
            lib_payload.files = _Save.data.files;
            //lib_payload.userFiles = _Save.data.user_files;
            //lib_payload.sharedFiles = _Save.data.shared_files;
            //lib_payload.worldFiles = _Save.data.world_files;

            bool loadTextures = true;
            bool loadThumbnails = true;
            bool generateThumbnails = true;
            
            #if UNITY_EDITOR
                loadTextures = Preset.loadTexturesInEditor;
                loadThumbnails = Preset.loadThumbnailsInEditor;
                generateThumbnails = Preset.generateThumbnailsInEditor;
            #endif
            
            Library.Load(lib_payload, Preset.defaultNullTexture, texturePacks, loadTextures, Preset.backlogTextures, loadThumbnails, generateThumbnails);
        }
        
        void SaveLibraryItems()
        {
            var payload = (LibraryPayload) Library.Save();
                _Save.data.directories = payload.directories;
                _Save.data.files = payload.files;
                _Save.data.user_files = payload.userFiles.Select(uf => (ushort)uf).ToArray();
                _Save.data.shared_files = payload.sharedFiles.Select(sf => (ushort)sf).ToArray();;
                _Save.data.world_files = payload.worldFiles.Select(wf => (ushort)wf).ToArray();;
        }

        public void Load(object data)
        {
            print("Load world data!");
        }

        #endregion
        
        #region Import

        protected override FileNavigator _Files
        {
            get => Files;
        }

        protected override void HandleImageImport(IEnumerable<string> images, POINT point)
        {
            bool multipleImages = images.Count() > 1;
                
            foreach (string image in images) {
                var info = new FileInfo(image);
                var path = info.FullName;

                bool exists = Library.RegisterFileInstance(path, Library.FileType.User);
                if (exists)
                    wand.AddBeacon(path, point, random: multipleImages); // Add beacon to scene via wand
                else
                    Debug.LogErrorFormat("File => {0} does not exist on user's desktop!", path);
            }
        }

        protected override void HandleBrainImport(BrainData brain, POINT point)
        {
            ImportNeueAgent(brain);
        }

        #endregion
    }
}
