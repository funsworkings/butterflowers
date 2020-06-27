using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    public UnityEvent onBeginGame;

    GameDataSaveSystem Save;

    [SerializeField] GameObject continuePanel;

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
        onBeginGame.Invoke();
    }

    public void ContinueGame()
    {
        onBeginGame.Invoke();
    }
}
