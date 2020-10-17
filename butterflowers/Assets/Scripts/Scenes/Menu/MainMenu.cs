using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using uwu;
using uwu.UI.Behaviors.Visibility;

public class MainMenu : MonoBehaviour
{
    public UnityEvent onBeginGame, onContinueGame;

    GameDataSaveSystem Save;

    [SerializeField] GameObject continuePanel;
    [SerializeField] ToggleOpacity opacity;

    bool @continue = false;

    void Awake()
    {
        Save = GameDataSaveSystem.Instance;
    }

    void OnEnable()
    {
        GameDataSaveSystem.onLoad += DisplayOptions;
    }

    void OnDisable()
    {
        GameDataSaveSystem.onLoad -= DisplayOptions;
    }

    void Start()
    {
        Save = GameDataSaveSystem.Instance;
        Save.LoadGameData(events: true);
    }

    void DisplayOptions(bool continue_available) 
    {
        continuePanel.SetActive(continue_available);
    }

    public void NewGame()
    {
        Save.ResetGameData();
        @continue = false;
        opacity.Hide();
    }

    public void ContinueGame()
    {
        @continue = true;
        opacity.Hide();
    }

    public void TriggerSelection()
    {
        if(@continue)
            onContinueGame.Invoke();
        else
            onBeginGame.Invoke();
    }
}
