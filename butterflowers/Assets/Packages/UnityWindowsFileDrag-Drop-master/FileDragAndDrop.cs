using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System.IO;
using butterflowersOS;
using butterflowersOS.Core;
using Neue.Agent.Brain.Data;
using Neue.Utilities;
using Shibuya24.Utility;
using uwu.Data;
using uwu.Extensions;
using uwu.IO;


public class FileDragAndDrop : MonoBehaviour
{
    void OnEnable ()
    {
        #if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
            // must be installed on the main thread to get the right thread id.
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        
        #elif UNITY_STANDALONE_OSX

        UniDragAndDrop.onDragAndDropFiles += OnFiles;
        UniDragAndDrop.Initialize();

        #endif
    }
    void OnDisable()
    {
        #if !UNITY_EDITOR_WIN && UNITY_STANDALONE_WIN
            UnityDragAndDropHook.UninstallHook();
        #elif UNITY_STANDALONE_OSX
            UniDragAndDrop.onDragAndDropFiles -= OnFiles;
        #endif
    }

    void OnFiles(string lFiles, int screenX, int screenY)
    {
        Debug.LogErrorFormat("Did receive files from mac drag & drop => {0} \nx:{1}\ny:{2}", lFiles, screenX, screenY);

        if (!string.IsNullOrEmpty(lFiles))
        {
            string[] files = JsonHelper.getJsonArray<string>(lFiles);
            POINT pt = new POINT(screenX*2, (Screen.height - screenY*2));

            if (files.Length > 0)
            {
                var receivers = FindObjectsOfType<MonoBehaviour>().OfType<IFileDragAndDropReceiver>();
                foreach (IFileDragAndDropReceiver receiver in receivers)
                {
                    receiver.ReceiveFiles(new List<string>(files), pt);
                }
            }
        }
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        var receivers = FindObjectsOfType<MonoBehaviour>().OfType<IFileDragAndDropReceiver>();
        foreach (IFileDragAndDropReceiver receiver in receivers) 
        {
            receiver.ReceiveFiles(aFiles, aPos);
        }
    }
}
