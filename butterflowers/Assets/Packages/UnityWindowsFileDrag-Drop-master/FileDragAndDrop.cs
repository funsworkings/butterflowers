using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using System.IO;
using butterflowersOS;
using butterflowersOS.Core;
using Neue.Agent.Brain.Data;
using uwu.Data;
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
        var images = GetImages(aFiles);
        var profile = GetProfile(aFiles);

        bool multipleImages = images.Count() > 1;
        foreach (string image in images) 
        {
            var info = new FileInfo(image);
            var path = info.FullName;
            
            bool exists = Lib.RegisterFileInstance(path, Library.FileType.User);
            if (exists)
                wand.AddBeacon(path, aPos, random:multipleImages); // Add beacon to scene via wand
            else
                Debug.LogErrorFormat("File => {0} does not exist on user's desktop!", path);
        }

        if (profile != null) // Detected a profile to import!
        {
            World.Instance.ImportNeueAgent(profile);
        }
    }

    IEnumerable<string> GetImages(List<string> aFiles)
    {
        
        
        return aFiles.Where(file =>
            Files.ExtensionMatchesFilter(Path.GetExtension(file).ToLowerInvariant())
        );
    }

    BrainData GetProfile(List<string> aFiles)
    {
        foreach (string aFile in aFiles) 
        {
            var ext = Path.GetExtension(aFile).ToLowerInvariant();
            if (ext == ".fns" || ext == ".FNS") // Matches 
            {
                BrainData dat = DataHandler.Read<BrainData>(aFile);
                if (dat != null) 
                {
                    if (dat.IsProfileValid()) 
                    {
                        return dat; // Break out of loop, successfully found file!
                    }
                }
            }
        }
        
        return null;
    }
}
