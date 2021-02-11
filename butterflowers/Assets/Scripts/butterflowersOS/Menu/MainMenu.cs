using System;
using System.Collections;
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

        [SerializeField] WorldPreset preset;
        [SerializeField] GameObject continueButton;
        [SerializeField] ToggleOpacity continuePanel;
        [SerializeField] MenuOption continueOption;
        [SerializeField] TMP_Text continueText;
        [SerializeField] ChooseUsername usernamePanel;
        [SerializeField] SceneAudioManager sceneAudio;

        public enum Route
        {
            NULL,
        
            NewGame,
            ContinueGame
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

            usernamePanel.Close();
            MoveToTheGame(1);
        }

        public void ContinueGame()
        {
            route = Route.ContinueGame;

            Close();

            int sceneIndex = 1;
            MoveToTheGame(sceneIndex);
        }

        public void Reset()
        {
            if(usernamePanel.IsVisible) usernamePanel.Close();

            route = Route.NULL;
            Open();
            onMainMenu.Invoke();
        }

        void DisplayOptions(bool previousSave)
        {
            Debug.LogWarningFormat("Main menu showed options! Save file exists => {0}", previousSaveExists);
            
            if (previousSave) 
            {
                float time = Save.data.sun.time;
                int days = Mathf.FloorToInt(preset.ConvertSecondsToDays(time));

                continueOption.DefaultText = string.Format("continue <size=45%>({0})</size>", days);
            }
            continueButton.SetActive(previousSave);
        }

        IEnumerator MovingToGame(int sceneIndex)
        {
            var opacity = (route == Route.ContinueGame) ? this.opacity : continuePanel;
            while (opacity.Visible) 
            {
                yield return null;
            }
            
            SceneLoader.Instance.GoToScene(sceneIndex, 0f, .1f);
        }
    
        #endregion

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
