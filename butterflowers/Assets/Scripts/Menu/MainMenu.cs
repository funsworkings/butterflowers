using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.SceneManagement;
using uwu;
using uwu.Scenes;
using uwu.UI.Behaviors.Visibility;

public class MainMenu : MonoBehaviour
{
    GameDataSaveSystem Save;
    
    // Properties

    [SerializeField] GameObject continuePanel;
    [SerializeField] Animator animation;
    [SerializeField] SceneUtils sceneUtils;

    public enum Route
    {
        NULL,
        
        NewGame,
        ContinueGame
    }

    public Route route = Route.NULL;

    bool previousSceneFileExists = false;
    bool previousBrainFileExists = false;
    
    void Start()
    {
        Save = GameDataSaveSystem.Instance;

        previousSceneFileExists = Save.LoadGameData<SceneData>();
        
        previousBrainFileExists = Save.LoadGameData<BrainData>("brain.fns");
        if (previousBrainFileExists) 
            previousBrainFileExists = !string.IsNullOrEmpty(Save.brainData.username);

        DisplayOptions(previousSceneFileExists && previousBrainFileExists);
    }

    void DisplayOptions(bool previousSave) 
    {
        continuePanel.SetActive(previousSave);
        animation.SetInteger("options", (previousSave)? 2:1);
    }
    
    #region Routes

    public void NewGame()
    {
        RecoverGameData();
        WipeGameData();
        
        route = Route.NewGame;
        animation.SetTrigger("new_game");
    }

    public void ContinueNewGame()
    {
        if (route != Route.NewGame) return;
        animation.SetTrigger("trigger_game");
    }

    public void ContinueGame()
    {
        route = Route.ContinueGame;
        animation.SetTrigger("trigger_game");
    }
    
    #endregion

    void RecoverGameData()
    {
        if(!previousSceneFileExists) Save.LoadGameData<SceneData>(createIfEmpty: true);
        if(!previousBrainFileExists) Save.LoadGameData<BrainData>("brain.fns", true);
    }
    
    void WipeGameData()
    {
        Save.WipeGameData<BrainData>("brain.fns");
        Save.WipeGameData<SceneData>(); 
    }

    public void MoveToTheGame()
    {
        if (route == Route.NULL) 
        {
            Debug.LogWarning("Ignore request to load scene when route is NULL!");
            return;
        }
        Debug.LogWarning("Move!");
        SceneManager.LoadScene(1);
    }
}
