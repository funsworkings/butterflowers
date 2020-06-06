using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;


public class GameDataSaveSystem : Singleton<GameDataSaveSystem>
{

    private const string savefile = "save.dat";

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

    void Awake()
    {
        
    }

    private void OnEnable()
    {
        LoadGameData();
    }

    private void OnDisable()
    {
        SaveGameData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGameData();
        else
            LoadGameData();
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
    }

    #endregion

}
