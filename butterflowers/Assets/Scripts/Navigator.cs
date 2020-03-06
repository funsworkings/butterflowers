using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;
using System.IO;
using System.Linq;

using Enviro = System.Environment;
using Filter = SimpleFileBrowser.FileBrowser.Filter;
using Files = SimpleFileBrowser.FileBrowserHelpers;


public class Navigator : MonoBehaviour
{
    public delegate void OnSuccessReceiveImage(byte[] data);
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



    const string display_header = "current path: {0}\ncurrent item: {1}";
    const string display_item   = "file -->  name: {0}  ext: {1}  type: {2}"; 


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

    public string DefaultPath { get { return Enviro.GetFolderPath(Enviro.SpecialFolder.Desktop); } }



    #region File Accessors

    public FileSystemEntry FirstFileInDirectory { get { return GetFirstFileInDirectory(type: typeFilter); } }
    public FileSystemEntry LastFileInDirectory { get { return GetLastFileInDirectory(type: typeFilter); } }
    public FileSystemEntry RandomFileInDirectory { get { return GetRandomFileInDirectory(type: typeFilter); } }


    public FileSystemEntry GetFileInDirectory(string directory = null, int index = 0, TypeFilter type = TypeFilter.All)
    {
        FileSystemEntry file = null;

        if (directory == null) directory = CurrentPath;

        bool isDirectory = isPathDirectory(directory);
        if (isDirectory)
        {
            var files = GetFilesInDirectory(directory, ofType: type);

            if(files.Length > 0)
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

    public FileSystemEntry GetLastFileInDirectory(string directory = null, TypeFilter type = TypeFilter.All)
    {
        if (directory == null) directory = CurrentPath;

        var files = GetFilesInDirectory(directory, ofType: type);
        return GetFileInArray(files, files.Length-1);
    }

    public FileSystemEntry GetRandomFileInDirectory(string directory = null, TypeFilter type = TypeFilter.All)
    {
        if (directory == null) directory = CurrentPath;

        var files = GetFilesInDirectory(directory, ofType: type);
        return GetFileInArray(files, Random.Range(0, files.Length));
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
                var entry = new FileSystemEntry(path, name, true);

                parent = entry;
            }
        }

        return parent;
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
                    if((allTypesFlag || typeFilter == 1)) // Directory
                        entries.Add(file);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
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
            foreach(FileSystemEntry file in m_files)
                items += ("\n" + string.Format(display_item, file.Name, file.Extension, (file.IsDirectory? "Directory":"File") ));
        }

        targetDisplay.text = header + "\n\n" + items;
    }


    public void Ascend()
    {
       
    }

    public void Descend()
    {

    }

    public void Left()
    {

    }

    public void Right()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        CurrentPath = DefaultPath;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            CurrentPath = DefaultPath;
    }   

    void OnSuccess(string path){
        Debug.Log("Successfully loaded file at path: " + path);

        if(onSuccessReceiveImage != null) {
            var bytes = FileBrowserHelpers.ReadBytesFromFile( FileBrowser.Result );
            onSuccessReceiveImage(bytes);
        }
    }

    void OnCancel(){

    }
}
