using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System.IO;
using uwu.IO;


public class FileDragAndDrop : MonoBehaviour
{
    Library Lib;
    FileNavigator Files;

    [SerializeField] Wand wand;

    List<string> log = new List<string>();
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

    void Start()
    {
        Lib = Library.Instance;
        Files = FileNavigator.Instance;
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (string s in aFiles) 
        {
            Debug.LogError(s);
            
            var info = new FileInfo(s);
            var path = info.FullName;
            
            var ext = System.IO.Path.GetExtension(s).ToLowerInvariant();
            bool success = Files.ExtensionMatchesFilter(ext);

            if (success) 
            {
                bool exists = Lib.RegisterFileInstance(path, aPos);
                if(exists) 
                    wand.AddBeacon(path, aPos); // Add beacon to scene via wand
                else
                    Debug.LogErrorFormat("File => {0} does not exist on user's desktop!", path);
            }
            else {
                Debug.LogErrorFormat("File => {0} does not match extensions!", path);
            }
        }
    }
}
