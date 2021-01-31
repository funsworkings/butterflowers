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

        public void ReceiveFiles(List<string> aFiles, POINT point)
        {
            var images = GetImages(aFiles);
            var profile = GetProfile(aFiles);


            if (filter == Filter.Images || filter == Filter.All) 
            {
                HandleImageImport(images, point);
            }

            if (filter == Filter.Brain || filter == Filter.All) 
            {
                if (profile != null) // Detected a profile to import!
                {
                    HandleBrainImport(profile, point);
                }
            }
        }

        IEnumerable<string> GetImages(List<string> aFiles)
        {
            return aFiles.Where(file =>
                _Files.ExtensionMatchesFilter(Path.GetExtension(file).ToLowerInvariant())
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
                    if (dat != null) {
                        if (dat.IsProfileValid()) {
                            return dat; // Break out of loop, successfully found file!
                        }
                    }
                }
            }
            
            return null;
        }

        protected virtual void HandleImageImport(IEnumerable<string> images, POINT point){}
        protected virtual void HandleBrainImport(BrainData brain, POINT point){}
    }
}