using UnityEngine;

public class AppPaused : MonoBehaviour
{
    bool isPaused = false;

    void Refresh()
    {
        string message = (isPaused) ? "paused" : "not paused";
        Debug.Log(message);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
        Refresh();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        Refresh();
    }
}