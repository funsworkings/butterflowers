using System.Collections.Generic;
using System.IO;
using System.Linq;
using B83.Win32;
using Neue.Agent.Brain.Data;
using UnityEngine;
using uwu.Data;
using uwu.IO;

namespace butterflowersOS.Core
{
    public abstract class Importer : MonoBehaviour, IFileDragAndDropReceiver
    {
        // External
        
        protected abstract FileNavigator _Files { get; }

        public enum Filter
        {
            Images,
            Brain,

            All
        }

        public Filter filter = Filter.All;

        [Header("Debug")] 
        public string debugFNS;

        public void ReceiveFiles(List<string> aFiles, POINT point)
        {
            var images = GetImages(aFiles);
            var profile = GetProfile(aFiles, out string path);


            if (filter == Filter.Images || filter == Filter.All) 
            {
                HandleImageImport(images, point);
            }

            if (filter == Filter.Brain || filter == Filter.All) 
            {
                if (profile != null) // Detected a profile to import!
                {
                    HandleBrainImport(path, profile, point);
                }
            }
        }

        IEnumerable<string> GetImages(List<string> aFiles)
        {
            return aFiles.Where(file =>
                _Files.ExtensionMatchesFilter(Path.GetExtension(file).ToLowerInvariant())
            );
        }

        BrainData GetProfile(List<string> aFiles, out string path)
        {
            foreach (string aFile in aFiles) 
            {
                var ext = Path.GetExtension(aFile).ToLowerInvariant();
                if (ext == ".fns" || ext == ".FNS") // Matches 
                {
                    BrainData dat = DataHandler.Read<BrainData>(aFile);
                    if (dat != null) 
                    {
                        if (dat.IsProfileValid()) {
                            path = aFile;
                            return dat; // Break out of loop, successfully found file!
                        }
                    }
                }
            }

            path = "";
            return null;
        }

        protected virtual void HandleImageImport(IEnumerable<string> images, POINT point){}
        protected virtual void HandleBrainImport(string path, BrainData brain, POINT point){}

        #region Debug bullshit

        [ContextMenu("Import debug FNS")]
        public void DebugImportFNS()
        {
            BrainData dat = DataHandler.Read<BrainData>(debugFNS);
            if (dat != null) 
            {
                Debug.LogWarning("Validate => " + dat.created_at);
                if (dat.IsProfileValid()) 
                {
                    OnDebugImportFNS(debugFNS, dat); // Break out of loop, successfully found file!
                }
            }
        }

        protected virtual void OnDebugImportFNS(string path, BrainData dat)
        {
            
        }
        
        #endregion
    }
}