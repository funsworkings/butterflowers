using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System.IO;
using butterflowersOS.Core;
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
        IEnumerable<string> validFiles = aFiles.Where(file =>
                Files.ExtensionMatchesFilter(Path.GetExtension(file).ToLowerInvariant())
        );

        bool multiple = validFiles.Count() > 1;
        
        foreach (string _file in validFiles) 
        {
            Debug.LogError(_file);
            
            var info = new FileInfo(_file);
            var path = info.FullName;
            
            bool exists = Lib.RegisterFileInstance(path, aPos);
            if (exists)
                wand.AddBeacon(path, aPos, random:multiple); // Add beacon to scene via wand
            else
                Debug.LogErrorFormat("File => {0} does not exist on user's desktop!", path);
        }
    }
}
