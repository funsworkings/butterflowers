using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Extensions;

public class WindowResize : MonoBehaviour
{
    public System.Action<int, int> onResize;
    
    int w = -1;
    int h = -1;

    bool @event = false;
    
    // Start is called before the first frame update
    void Start()
    {
        w = Screen.width;
        h = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        int _w = Screen.width;
        int _h = Screen.height;

        if (_w != w || _h != h) 
        {
            w = _w;
            h = _h;

            @event = true;
        }
    }

    void LateUpdate()
    {
        if (@event) 
        {
            if(onResize != null)
                onResize(w, h);

            @event = false;
        }
    }
}
