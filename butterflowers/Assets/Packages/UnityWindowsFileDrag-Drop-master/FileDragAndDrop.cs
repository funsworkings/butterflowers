using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System.IO;
using butterflowersOS;
using butterflowersOS.Core;
using Neue.Agent.Brain.Data;
using Neue.Utilities;
using uwu.Data;
using uwu.IO;


public class FileDragAndDrop : MonoBehaviour
{
    void OnEnable ()
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // must be installed on the main thread to get the right thread id.
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnFiles;
        #endif
    }
    void OnDisable()
    {
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            UnityDragAndDropHook.UninstallHook();
        #endif
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
