using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using System.IO;
using System.Linq;

using Enviro = System.Environment;
using UnityEngine.Networking;
using Filter = SimpleFileBrowser.FileBrowser.Filter;
using Files = SimpleFileBrowser.FileBrowserHelpers;


public class FileNavigator : MonoBehaviour
{
    public static FileNavigator Instance = null;

    #region Events

    public System.Action onRefresh;

    #endregion

    #region Internal

    public enum FileType { Normal, Directory, All };

    #endregion

    #region Attributes

    [SerializeField] Enviro.SpecialFolder root;
    [SerializeField] string[] filters = new string[] { };

    #endregion

    #region Collections

    [SerializeField] string path = "";
    [SerializeField] FileSystemEntry folder = null;

    Dictionary<string, FileSystemEntry> fileLookup = new Dictionary<string, FileSystemEntry>();
    Dictionary<string, FileSystemEntry> directoryLookup = new Dictionary<string, FileSystemEntry>();

    [SerializeField] FileSystemEntry[] files = new FileSystemEntry[] { };
    [SerializeField] FileSystemEntry[] directories = new FileSystemEntry[] { };
    
    #endregion

    #region Monobehaviour callbacks

    void Awake()
    {
        Instance = this;
    }

    void OnEnable(){
        if(string.IsNullOrEmpty(path))
            path = Enviro.GetFolderPath(root); // Set to root folder if path is null
    }

    void OnApplicationPause(bool paused) {
        if(!paused)
            Refresh(); // Refresh all files in directory
    }

    void OnApplicationFocus(bool focused)
    {
        if(focused)
            Refresh();
    }


    #endregion

    #region Instance helpers

    public bool IsFileVisible(string filename = null)
    {
        if (string.IsNullOrEmpty(filename)) return false;
        return fileLookup.ContainsKey(filename);
    }

	#endregion

	#region Helpers

	public static bool isPathDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                return attr.HasFlag(FileAttributes.Directory);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        return false;
    }

    public static FileSystemEntry GetParent(string path)
    {
        FileSystemEntry parent = null;

        if (isPathDirectory(path))
        {
            DirectoryInfo par = Directory.GetParent(path);
            if (par != null)
            {
                var name = System.IO.Path.GetDirectoryName(path);
                var entry = new FileSystemEntry(par.FullName, name, true);

                parent = entry;
            }
        }

        return parent;
    }

    public static FileSystemEntry GetFileInDirectory(string directory = null, int index = 0, FileType type = FileType.All)
    {
        FileSystemEntry file = null;

        bool isDirectory = isPathDirectory(directory);
        if (isDirectory)
        {
            var files = GetFilesInDirectory(directory, type: type);

            if (files.Length > 0)
            {
                if (index < 0)
                    index = 0;
                else if (index > files.Length - 1)
                    index = files.Length - 1;

                file = files[index];
            }
        }

        return file;
    }

    public static FileSystemEntry[] GetFilesInDirectory(string path,  FileType type = FileType.All, string[] filters = null)
    {
        List<FileSystemEntry> entries = new List<FileSystemEntry>();

        bool allTypes = (type == FileType.All);

        FileSystemEntry[] files = Files.GetEntriesInDirectory(path);
        if (files != null)
        {
            foreach (FileSystemEntry file in files)
            {
                try
                {
                    var directory = (file.IsDirectory);
                    if (!directory) // Normal file
                    {
                        if ((allTypes || type == FileType.Normal))
                        {
                            if(filters == null || filters.Length == 0)
                                entries.Add(file); // Automatically add filters to returned 
                            else{
                                var extension = file.Extension.ToLowerInvariant();
                                if (filters.Contains<string>(extension))
                                    entries.Add(file);
                            }
                        }
                    }
                    else
                    {
                        if ((allTypes || type == FileType.Directory)) // Directory
                            entries.Add(file);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        return entries.ToArray();
    }

    public string[] GetPathsFromFiles(FileSystemEntry[] files)
    {
        List<string> temp = new List<string>();
        for (int i = 0; i < files.Length; i++) 
            temp.Add(files[i].Path);

        return temp.ToArray();
    }

    #endregion

    #region Operations

    public void Refresh(){
        files = GetFilesInDirectory(path, FileType.Normal, filters);
        directories = GetFilesInDirectory(path, FileType.Directory);

        fileLookup = new Dictionary<string, FileSystemEntry>();
        for(int i = 0; i < files.Length; i++){
            var file = files[i];

            try {
                var name = file.Name;
                fileLookup.Add(name, file);
            }
            catch(System.Exception e){
                Debug.LogWarning(e);
            }
        }

        directoryLookup = new Dictionary<string, FileSystemEntry>();
        for(int i = 0; i < directories.Length; i++){
            var directory = directories[i];

            try {
                directoryLookup.Add(directory.Name, directory);
            }
            catch(System.Exception e){
                Debug.LogWarning(e);
            }
        }

        Debug.LogFormat("Refreshed navigator~  FILES:{0} | DIRECTORIES:{1}", files.Length, directories.Length);
        if(onRefresh != null)
            onRefresh();
    }

    public bool SetPath(string path){
        bool refresh = (this.path != path);
        if(!Directory.Exists(path))
            return false; // Ignore path override when directory is invalid

        this.path = path; folder = new FileSystemEntry(path, path, true);
        if(refresh)
            Refresh();

        return true;
    }

    public void Ascend()
    {
        var directory = GetParent(path);
        if (directory != null) {
            SetPath(directory.Path);
        }
    }

    public void Descend(string directory)
    {
        if(string.IsNullOrEmpty(directory)) return;
        if(!directoryLookup.ContainsKey(directory)) return;

        SetPath(directory);
    }

    #endregion

    #region Accessors

    public FileSystemEntry[] GetFiles(){ return files; }
    public string[] GetPaths() { return fileLookup.Keys.ToArray(); }
    public FileSystemEntry[] GetDirectories() { return directories; }
    public FileSystemEntry   GetParent(){ return GetParent(path); }

    #endregion
}
