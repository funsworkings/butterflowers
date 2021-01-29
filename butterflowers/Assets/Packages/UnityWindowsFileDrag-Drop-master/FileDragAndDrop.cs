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
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
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
