using System;
using System.Collections;
using System.IO;
using butterflowersOS.Presets;
using butterflowersOS.UI;
using Neue.Agent.Brain.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using uwu;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;
using UnityEngine.Events;

namespace butterflowersOS.Menu
{
    public class MainMenu : GenericMenu
    {
        GameDataSaveSystem Save;
        
        // Events

        public UnityEvent onMainMenu, onUserMenu;
    
        // Properties

        ToggleOpacity opacity;

        [SerializeField] WorldPreset preset = null;
        [SerializeField] GameObject continueButton = null, aiButton = null;
        [SerializeField] ToggleOpacity continuePanel = null;
        [SerializeField] MenuOption continueOption = null;
        [SerializeField] TMP_Text continueText;
        [SerializeField] ChooseUsername usernamePanel = null;
        [SerializeField] SettingsMenu settingsPanel = null;
        [SerializeField] SceneAudioManager sceneAudio = null;
        [SerializeField] bool disposePreviousVersion = false;

        public enum Route
        {
            NULL,
        
            NewGame,
            ContinueGame,
            ContinueAI
        }

        public Route route = Route.NULL;

        bool previousSaveExists = false;

        void Awake()
        {
            opacity = GetComponent<ToggleOpacity>();
        }

        protected override void Start()
        {
            Save = GameDataSaveSystem.Instance;
            
            Time.timeScale = 1f;
            Loader.Instance.Dispose();

            if (Save.load)
                previousSaveExists = (Save.data != null);
            else
                previousSaveExists = Save.LoadGameData<GameData>();

            base.Start();
            
            Reset();
        }

        #region Menu
        
        protected override void DidOpen()
        {
            route = Route.NULL;
            DisplayOptions(previousSaveExists);
            
            opacity.Show();   
        }

        protected override void DidClose()
        {
            opacity.Hide();
        }
        
        #endregion
    
        #region Routes

        public void NewGame()
        {
            route = Route.NewGame;

            Close();
            usernamePanel.Open();
            onUserMenu.Invoke();
        }

        public void ContinueNewGame(string input)
        {
            if (route != Route.NewGame) return;
            
            RecoverGameData();
            WipeGameData();

            Save.username = input;
            PlayerPrefs.SetString("_Username", input);
            
            Save.SaveGameData(); // Save game data with username

            usernamePanel.Dispose();
            MoveToTheGame(1);
        }

        public void ContinueGame()
        {
            route = Route.ContinueGame;

            Close();

            int sceneIndex = 1;
            MoveToTheGame(sceneIndex);
        }

        public void ContinueAI()
        {
            route = Route.ContinueAI;

            Close();

            int sceneIndex = 2;
            MoveToTheGame(sceneIndex);
        }

        public void GoToSettings()
        {
            Close();
            settingsPanel.Open();
        }

        public void Reset()
        {
            if(usernamePanel.IsVisible) usernamePanel.Close();
            if(settingsPanel.IsVisible) settingsPanel.Close();

            route = Route.NULL;
            Open();
            onMainMenu.Invoke();
        }

        void DisplayOptions(bool previousSave)
        {
            Debug.LogWarningFormat("Main menu showed options! Save file exists => {0}", previousSaveExists);

            bool showContinue = previousSave;
            
            if (previousSave) 
            {
                float time = Save.data.sun.time;
                int days = Mathf.FloorToInt(preset.ConvertSecondsToDays(time));

                continueOption.DefaultText = string.Format("continue <size=45%>({0})</size>", days);

                string prev_version = Save.data.BUILD_VERSION;
                string curr_version = Application.version;

                if (prev_version != curr_version && disposePreviousVersion) 
                {
                    DisposeVersionData();
                    showContinue = false; // Versions don't match
                }
            }
            
            continueButton.SetActive(showContinue);

            bool hasAIAccess = PlayerPrefs.GetInt(Constants.AIAccessKey, 0) == 1;
            aiButton.SetActive(hasAIAccess);
        }

        IEnumerator MovingToGame(int sceneIndex)
        {
            var opacity = (route == Route.ContinueGame) ? this.opacity : continuePanel;
            while (opacity.Visible) 
            {
                yield return null;
            }
            
            SceneLoader.Instance.GoToScene(sceneIndex);
        }
    
        #endregion

        void DisposeVersionData()
        {
            var thumbnailDir = Path.Combine(Path.GetFullPath(Application.persistentDataPath), "_thumbnails");
            if (Directory.Exists(thumbnailDir)) // Has thumbnails from previous version
            {
                var files = Directory.GetFiles(thumbnailDir);
                foreach (string file in files) 
                {
                    File.Delete(file); // Delete previous thumbnails
                }
            }
        }

        void RecoverGameData()
        {
            if(!previousSaveExists) Save.LoadGameData<GameData>(createIfEmpty: true);
        }
    
        void WipeGameData()
        {
            Save.WipeGameData<GameData>(); 
        }

        public void MoveToTheGame(int sceneIndex)
        {
            if (route == Route.NULL) 
            {
                Debug.LogWarning("Ignore request to load scene when route is NULL!");
                return;
            }
            
            sceneAudio.FadeOut(); // Fade scene audio OUT
           
            StartCoroutine("MovingToGame", sceneIndex);
        }
    }
}
