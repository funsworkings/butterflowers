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


public class Navigator : MonoBehaviour
{
    public delegate void OnSuccessReceiveImage(Texture data);
    public static event OnSuccessReceiveImage onSuccessReceiveImage;

    public delegate void OnFailReceiveImage();
    public static event OnFailReceiveImage onFailReceiveImage;


    [SerializeField] TMPro.TMP_Text targetDisplay;


    [SerializeField] string m_currentPath = "";
    [SerializeField] FileSystemEntry m_currentItem;

    [SerializeField] string[] m_filters = new string[] { ".jpg", ".png" };

    public enum TypeFilter { Normal, Directory, All };
    public TypeFilter typeFilter = TypeFilter.All;

    [SerializeField] FileSystemEntry[] m_files = new FileSystemEntry[] { };


    bool read = false;

    public List<string> visited = new List<string>();
    public bool allowRepeats = false;

    const string display_header = "current path: {0}\ncurrent item: {1}";
    const string display_item = "file -->  name: {0}  ext: {1}  type: {2}";


    public string CurrentPath {
        get { return m_currentPath; }
        set
        {
            m_currentPath = value;

            RefreshFilesInDirectory();
            CurrentItem = RandomFileInDirectory;

            RefreshDisplay();
        }
    }

    public FileSystemEntry CurrentItem
    {
        get { return m_currentItem; }
        set { m_currentItem = value; }
    }



    private const string rootDirectory = "\ud835\udcb7\ud835\udcca\ud835\udcc9\ud835\udcc9\ud835\udc52\ud835\udcc7\ud835\udcbb\ud835\udcc1\ud835\udc5c\ud835\udccc\ud835\udc52\ud835\udcc7\ud835\udcc8";
    public string DefaultPath { 
        get 
        { 
            string dir = Path.Combine(Enviro.GetFolderPath(Enviro.SpecialFolder.Desktop), rootDirectory);
                FileUtils.EnsureDirectory(dir); // Make sure butterflowers dir exists

            return dir;
        } 
    }




    #region File Accessors

    public FileSystemEntry FirstFileInDirectory { get { return GetFirstFileInDirectory(type: TypeFilter.All); } }
    public FileSystemEntry FirstDirectoryInDirectory { get { return GetFirstFileInDirectory(type: TypeFilter.Directory); } }

    public FileSystemEntry LastFileInDirectory { get { return GetLastFileInDirectory(type: TypeFilter.All); } }
    public FileSystemEntry LastDirectoryInDirectory { get { return GetLastFileInDirectory(type: TypeFilter.Directory); } }

    public FileSystemEntry RandomFileInDirectory { get { return GetRandomFileInDirectory(type: TypeFilter.All); } }
    public FileSystemEntry RandomDirectoryInDirectory { get { return GetRandomFileInDirectory(type: TypeFilter.Directory); } }


    public FileSystemEntry GetFileInDirectory(string directory = null, int index = 0, TypeFilter type = TypeFilter.All)
    {
        FileSystemEntry file = null;

        if (directory == null) directory = CurrentPath;

        bool isDirectory = isPathDirectory(directory);
        if (isDirectory)
        {
            var files = GetFilesInDirectory(directory, ofType: type);

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

    /// <summary> Get file from pre-combed array of files </summary>
    public FileSystemEntry GetFileInArray(FileSystemEntry[] files, int index = 0)
    {
        FileSystemEntry file = null;

        if (files.Length > 0)
        {
            if (index < 0)
                index = 0;
            else if (index > files.Length - 1)
                index = files.Length - 1;

            file = files[index];
        }

        return file;
    }

    public FileSystemEntry GetFirstFileInDirectory(string directory = null, TypeFilter type = TypeFilter.All)
    {
        if (directory == null) directory = CurrentPath;

        return GetFileInDirectory(directory, 0, type);
    }

    public FileSystemEntry GetFirstDirectoryInDirectory(string directory = null)
    {
        return GetFirstFileInDirectory(directory, TypeFilter.Directory);
    }

    public FileSystemEntry GetLastFileInDirectory(string directory = null, TypeFilter type = TypeFilter.All)
    {
        if (directory == null) directory = CurrentPath;

        var files = GetFilesInDirectory(directory, ofType: type);
        return GetFileInArray(files, files.Length - 1);
    }

    public FileSystemEntry GetLastDirectoryInDirectory(string directory = null)
    {
        return GetLastFileInDirectory(directory, TypeFilter.Directory);
    }

    public FileSystemEntry GetRandomFileInDirectory(string directory = null, TypeFilter type = TypeFilter.All)
    {
        if (directory == null) directory = CurrentPath;

        var files = GetFilesInDirectory(directory, ofType: type);
        return GetFileInArray(files, Random.Range(0, files.Length));
    }

    public FileSystemEntry GetRandomDirectoryInDirectory(string directory = null)
    {
        return GetRandomFileInDirectory(directory, TypeFilter.Directory);
    }

    #endregion


    #region Helpers

    public bool isPathDirectory(string path)
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

    public FileSystemEntry GetParent(string path = null)
    {
        FileSystemEntry parent = null;

        if (path == null) path = CurrentPath;

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

    FileSystemEntry[] GetDirectoriesInDirectory(string path, bool includesParent = true)
    {
        return GetFilesInDirectory(path, includesParent, TypeFilter.Directory);
    }

    FileSystemEntry[] GetFilesInDirectory(string path, bool includesParent = true, TypeFilter ofType = TypeFilter.All)
    {
        List<FileSystemEntry> entries = new List<FileSystemEntry>();


        int typeFilter = (int)ofType;
        bool allTypesFlag = (typeFilter == 2);

        if ((allTypesFlag || typeFilter == 1) && includesParent)
        {
            FileSystemEntry parent = GetParent(path);
            if (parent != null)
                entries.Add(parent);
        }


        FileSystemEntry[] files = Files.GetEntriesInDirectory(CurrentPath);

        if (files != null)
        {
            foreach (FileSystemEntry file in files)
            {
                try
                {
                    var directory = (file.IsDirectory);

                    if (!directory) // Normal file
                    {
                        if ((allTypesFlag || typeFilter == 0))
                        {
                            var extension = file.Extension.ToLowerInvariant();
                            if (m_filters.Contains<string>(extension))
                                entries.Add(file);
                        }
                    }
                    else
                    {
                        if ((allTypesFlag || typeFilter == 1)) // Directory
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

    #endregion

    void RefreshFilesInDirectory()
    {
        m_files = GetFilesInDirectory(CurrentPath, ofType: typeFilter);
    }

    void RefreshDisplay() {
        string header = string.Format(display_header, CurrentPath, CurrentItem.Name);
        string items = "";


        if (m_files == null || m_files.Length == 0)
            items = "No items available in directory!";
        else
        {
            foreach (FileSystemEntry file in m_files)
                items += ("\n" + string.Format(display_item, file.Name, file.Extension, (file.IsDirectory ? "Directory" : "File")));
        }

        targetDisplay.text = header + "\n\n" + items;
    }


    #region Navigation

    public void Ascend()
    {
        var directory = GetParent();
        if (directory != null)
            CurrentPath = directory.Path;
    }

    public void Descend()
    {
        var directory = RandomDirectoryInDirectory;
        if (directory != null)
            CurrentPath = directory.Path;
    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        CurrentPath = DefaultPath;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Refresh();
        if (Input.GetKeyDown(KeyCode.Escape))
            CurrentPath = DefaultPath;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            Ascend();
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            Descend();
    }

    public void Refresh(FileSystemEntry file = null)
    {
        CurrentPath = CurrentPath;

        var img = file;
        if(img == null)
            img = GetRandomFileInDirectory(null, TypeFilter.Normal);

        if (img != null)
        {
            if (!allowRepeats)
            {
                if (visited.Contains(img.Path))
                    return;
            }

            OnSuccess(img.Path);
        }
    }

    void OnSuccess(string path){
        Debug.Log("Successfully loaded file at path: " + path);
        if (!read)
        {
            StartCoroutine(ReadBytesFromFile(path));
            read = true;

            if (visited.Count == m_files.Length)
                visited = new List<string>();

            visited.Add(path);
        }
    }

    IEnumerator ReadBytesFromFile(string file)
    {
        Debug.Log(file);

        UnityWebRequest req = UnityWebRequestTexture.GetTexture("file://" + file);
            req.SendWebRequest();

        while (!req.isDone)
        {
            Debug.Log(req.downloadProgress*100f + "%");
            yield return null;
        }
        read = false;

        if (!(req.isHttpError || req.isNetworkError))
        {
            Texture texture = ((DownloadHandlerTexture)req.downloadHandler).texture;
            if (onSuccessReceiveImage != null)
                onSuccessReceiveImage(texture);
        }
    }

    void OnCancel(){

    }
}
