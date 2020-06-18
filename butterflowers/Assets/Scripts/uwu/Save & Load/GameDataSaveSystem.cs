using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;
using Wizard;

public class GameDataSaveSystem : Singleton<GameDataSaveSystem>
{

    private const string savefile = "save.dat";
    private const float refreshrate = 15f;

    private bool m_autosave = false;
    public bool autosave {
        get
        {
            return m_autosave;
        }
        set
        {
            if (autosave != value) 
            {
                if (!value) StopCoroutine("Autosave");
                else StartCoroutine("Autosave");
            }
            m_autosave = value;
        }
    }


    [SerializeField] GameData m_data = null;
    public GameData data
    {
        get
        {
            return m_data;
        }
    }

    bool m_load = false;
    public bool load
    {
        get
        {
            return m_load;
        }
    }

    #region External access

    public float time {
        get {
            return (data == null)? 0f:data.time;
        }
        set {
            data.time = value;
        }
    }

    public int chapter 
    {
        get
        {
            return data.chapter;
        }
        set
        {
            data.chapter = value;
        }
    }

    public string[] discovered {
        get
        {
            var discoveries = (data == null) ? null : data.discoveries;
            if (discoveries == null) {
                discoveries = new string[] { };
                discovered = discoveries;
            }
            return discoveries;
        }
        set
        {
            data.discoveries = value;
        }
    }

    public BeaconData[] beaconData {
        get
        {
            return (data == null) ? new BeaconData[] { } : data.beacons;
        }
    }

    public Beacon[] beacons {
        set
        {
            var dat = new List<BeaconData>();
            for (int i = 0; i < value.Length; i++) {
                var beacon = value[i];
                var parsed = new BeaconData(beacon.file, beacon.type, beacon.visible);

                dat.Add(parsed);
            }

            data.beacons = dat.ToArray();
        }
    }

    public bool nestOpen {
        get
        {
            return (data == null) ? false : data.nestopen;
        }
        set
        {
            data.nestopen = value;
        }
    }

    public int dialogueNode {
        get
        {
            return (data == null) ? -1 : data.dialoguenode;
        }
        set
        {
            data.dialoguenode = value;
        }
    }

    public int[] dialogueVisited {
        get
        {
            return (data == null) ? new int[] { } : data.dialoguevisited;
        }
        set
        {
            data.dialoguevisited = value;
        }
    }

    public float enviro_knowledge {
        get
        {
            return (data == null) ? 0f : data.enviro_knowledge;
        }
        set
        {
            data.enviro_knowledge = value;
        }
    }

    public Knowledge[] file_knowledge {
        get
        {
            return (data == null) ? new Knowledge[] { } : data.file_knowledge;
        }
        set
        {
            data.file_knowledge = value;
        }
    }

    public string[] shared_files {
        get
        {
            return (data == null) ? new string[] { } : data.shared_files;
        }
        set
        {
            data.shared_files = value;
        }
    }

    #endregion

    #region Fetch data path


    string m_dataPath = null;
    public string dataPath
    {
        get
        {
            string dir = Application.persistentDataPath + DataPaths.DATA_PATH;
            FileUtils.EnsureDirectory(dir);

            if (m_dataPath == null)
                m_dataPath = Path.Combine(dir, savefile);

            Debug.LogFormat("Save data path = {0}", m_dataPath);
            return m_dataPath;
        }
    }

    #endregion

    private void OnEnable()
    {
        LoadGameData();
    }

    private void OnDisable()
    {
        SaveGameData();
        autosave = false;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) {
            SaveGameData();
            autosave = false;
        }
        else
            LoadGameData();
    }

    IEnumerator Autosave()
    {
        while (true) 
        {
            yield return new WaitForSeconds(refreshrate);
            SaveGameData();
        }
    }

    void SaveGameData()
    {
        onSaveGameData();

        DataHandler.Write<GameData>(m_data, dataPath);
    }

    public void LoadGameData()
    {
        GameData load = DataHandler.Read<GameData>(dataPath);
        if (load == null)
        {
            Debug.LogWarning("No save file located, initializing data file...");
            m_data = new GameData();
        }
        else
            m_data = load;

        onLoadGameData();
        m_load = true;
    }

	#region Save/load callbacks

	void onSaveGameData()
    {
        Debug.LogFormat("~~Save file was SAVED on {0}~~", System.DateTime.Now.ToShortTimeString());
    }

    void onLoadGameData()
    {
        Debug.LogFormat("~~Save file was LOADED on {0}~~", System.DateTime.Now.ToShortTimeString());
        autosave = (refreshrate > 0f);
    }

    #endregion

}
