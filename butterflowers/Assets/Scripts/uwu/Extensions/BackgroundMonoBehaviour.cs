using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMonoBehaviour : MonoBehaviour
{
    protected bool focused = true;
    
    void OnApplicationFocus(bool hasFocus)
    {
        focused = hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        focused = !pauseStatus;
    }
}
