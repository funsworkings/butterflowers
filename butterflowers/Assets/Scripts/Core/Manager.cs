using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = SimpleFileBrowser.FileSystemEntry;
using Memory = Wizard.Memory;
using System.Runtime.InteropServices;

public class Manager : Spawner
{
    public static Manager Instance = null;

    #region External

    [SerializeField] Settings.WorldPreset Preset;
    [SerializeField] Wizard.MemoryBank WizardMemoryBank;

    GameDataSaveSystem Save = null;
    Library Library = null;
    FileNavigator Files = null;
    Discovery Discovery = null;
    MotherOfButterflies Butterflowers = null;
    Sun Sun = null;
    Nest Nest = null;
    Quilt Quilt = null;
    Wizard.Controller Wizard = null;
    Loading Loader = null;

	#endregion

	#region Attributes

	[SerializeField] int maxBeacons = 100;

    #endregion

    #region Collections

    Dictionary<string, List<Beacon>> beacons = new Dictionary<string, List<Beacon>>();
    List<Beacon> allBeacons = new List<Beacon>();

    #endregion

    #region Monobehaviour callbacks

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
        spawnOnAwake = false;

        Save = GameDataSaveSystem.Instance;
    }

    protected override void Start(){
        base.Start();

        Library = Library.Instance;
        Library.WizardFiles = WizardMemoryBank;

        Discovery = Discovery.Instance;
        Discovery.Preset = Preset;

        Files = FileNavigator.Instance;
        Sun = Sun.Instance;
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Butterflowers = FindObjectOfType<MotherOfButterflies>();
        Wizard = FindObjectOfType<Wizard.Controller>();
        Loader = FindObjectOfType<Loading>();

        StartCoroutine("Initialize");
    }

    protected override void OnDestroy(){
        base.OnDestroy();

        UnsubscribeToEvents();
    }

    #endregion

    #region Internal

    IEnumerator Initialize()
    {
        while (!Save.load) yield return null;
        while (!Discovery.load) yield return null;

        SubscribeToEvents();

        var dat = Save.beaconData; Debug.LogFormat("Found {0} beacons from SAVE", dat.Length);
        var refresh = (dat == null || dat.Length == 0) || !Preset.persist;

        Library.Initialize(refresh);

        while (Library.loadprogress < 1f) 
        {
            Loader.progress = Library.loadprogress;
            yield return null;
        }
        Loader.progress = 1f;

        RestoreNest();

        if (!refresh) {
            RestoreBeacons(dat); // Restore from save file
            DeleteDeprecatedBeacons();
        }
        else
            RefreshBeacons();

        Sun.active = true; // Set sun into motion
    }

    void SubscribeToEvents()
    {
        Sun.onCycle += Advance;

        Nest.onOpen.AddListener(onNestStateChanged);
        Nest.onClose.AddListener(onNestStateChanged);
        Nest.onAddBeacon += onIngestBeacon;
        Nest.onRemoveBeacon += onReleaseBeacon;

        Beacon.OnRegister += onRegisterBeacon;
        Beacon.OnUnregister += onUnregisterBeacon;
        Beacon.Discovered += onDiscoveredBeacon;

        Quilt.onDisposeTexture += onDisposeQuiltTexture;
    }

    void UnsubscribeToEvents()
    {
        Sun.onCycle -= Advance;

        Nest.onOpen.RemoveListener(onNestStateChanged);
        Nest.onClose.RemoveListener(onNestStateChanged);
        Nest.onAddBeacon -= onIngestBeacon;
        Nest.onRemoveBeacon -= onReleaseBeacon;

        Beacon.OnRegister -= onRegisterBeacon;
        Beacon.OnUnregister -= onUnregisterBeacon;
        Beacon.Discovered -= onDiscoveredBeacon;

        Quilt.onDisposeTexture -= onDisposeQuiltTexture;
    }

    #endregion

    #region Spawner overrides

    protected override void CalculateBounds()
    {
        m_center = Vector3.zero;
        m_extents = root.GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    protected override void DecideRotation(ref Quaternion rot)
    {
        rot = transform.rotation;
    }

    #endregion

    #region Sun callbacks

    void Advance()
    {
        bool success = Nest.Close(); // Close nest
        //if(success) Butterflowers.KillButterflies(); // Kill all butterflies (if nest was closed)

        RefreshBeacons(); // Reset all beacons
    }

    #endregion

    #region Nest operations

    void RestoreNest()
    {
        if (Save.nestOpen) Nest.Open();
        else Nest.Close();
    }

	#endregion

	#region Beacon operations

	public Beacon CreateBeacon(string path, Beacon.Type type = Beacon.Type.Desktop)
    {
        var instance = InstantiatePrefab(); // Create new beacon prefab

        var beacon = instance.GetComponent<Beacon>();
        beacon.file = path;
        beacon.fileEntry = null;
        beacon.origin = instance.transform.position;
        beacon.type = type;

        var discovered = Discovery.HasDiscoveredFile(path);
        beacon.discovered = discovered; // Set if beacon has been discovered

        beacon.visible = true;

        beacon.Register();
        beacon.Appear();

        return beacon;
    }

    public void DeleteBeacon(Beacon beacon, bool overridenest = false)
    {
        if (overridenest) 
            beacon.Delete();
        else {
            if (!Nest.HasBeacon(beacon))
                beacon.Delete();
        }
    }

    // If beacon is in subset, IGNORE
    // If beacon is not in subset, DELETE

    void RefreshBeacons()
    {
        if (Library.loadprogress < 1f) return; // Ignore refresh if library not finished

        DeleteDeprecatedBeacons();

        var files = Files.GetFiles();
        var subset = files.PickRandomSubset<FileSystemEntry>(maxBeacons).ToList(); // Target beacons        

        var current = (beacons != null)? beacons.Keys.ToList(): new List<string>();
        if (current.Count > 0) {

            var target = Files.GetPathsFromFiles(subset.ToArray());

            for (int i = 0; i < current.Count; i++) 
            {
                var path = current[i];
                if (!target.Contains(current[i])) {
                    var bs = beacons[path].ToArray();
                    for (int j = 0; j < bs.Length; j++) {
                        var b = bs[j];
                        DeleteBeacon(b);
                    }

                }
            }
        }

        CreateBeaconsFromFiles(subset.ToArray()); // Instantiate all target files
    }

    void RestoreBeacons(BeaconData[] data)
    {
        Debug.LogFormat("Recovered {0} beacons from save!", data.Length);
        for (int i = 0; i < data.Length; i++) {
            var beacon = data[i];

            string p = beacon.path;
            Beacon.Type t = (Beacon.Type)beacon.type;
            bool v = beacon.visible;

            var instance = CreateBeacon(p, t);
            if (!v) onDiscoveredBeacon(instance); // Send to nest if immediately found
        }
    }

    void CreateBeaconsFromFiles(FileSystemEntry[] files)
    {
        for (int i = 0; i < files.Length; i++) {
            var file = files[i];
            var path = file.Path;

            bool exists = (beacons.ContainsKey(path));
            if (exists) {
                exists = false;

                var bs = beacons[path];
                for (int j = 0; j < bs.Count; j++) {
                    if (!Nest.HasBeacon(bs[j])) {
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists) {
                var beacon = CreateBeacon(path);
                beacon.fileEntry = file;
            }
        }
    }

    void CreateBeaconsFromMemories(Memory[] memories)
    {
        for (int i = 0; i < memories.Length; i++) {
            var memory = memories[i];
            var path = memory.name;

            if (!string.IsNullOrEmpty(path)) {
                bool exists = (beacons.ContainsKey(path));
                if (exists) {
                    exists = false;

                    var bs = beacons[path];
                    for (int j = 0; j < bs.Count; j++) {
                        if (!Nest.HasBeacon(bs[j])) {
                            exists = true;
                            break;
                        }
                    }
                }

                if (!exists) {
                    var beacon = CreateBeacon(path);
                    beacon.fileEntry = null;
                }
            }
        }
    }

    void DeleteDeprecatedBeacons()
    {
        var current = (beacons != null) ? beacons.Keys.ToList() : new List<string>();

        var path = "";
        for (int i = 0; i < current.Count; i++) {
            path = current[i];

            if (!Library.ContainsFile(path)) {
                var bs = beacons[path].ToArray();
                foreach (Beacon b in bs)
                    DeleteBeacon(b, true);
            }
        }
    }

    public Beacon CreateBeaconForWizard(Texture2D texture)
    {
        string name = texture.name;
        return CreateBeacon(name, Beacon.Type.Wizard);
    }

    public bool ActivateRandomBeacon()
    {
        var beacons = this.beacons.Values;
        IEnumerable<Beacon> inactive = allBeacons.Where(beacon => !Nest.HasBeacon(beacon));

        int count = inactive.Count();
        if (count == 0) return false;

        var _beacon = inactive.ElementAt(Random.Range(0, count));
        _beacon.Discover();

        return true;
    }

    #endregion

    #region Beacon callbacks

    void onRegisterBeacon(Beacon beacon)
    {
        var file = beacon.file;
        if (beacons.ContainsKey(file)) {
            var list = beacons[file];
            list.Add(beacon);

            beacons[file] = list;
        }
        else
            beacons.Add(file, new List<Beacon>(new Beacon[] { beacon }));

        allBeacons.Add(beacon);
        Save.beacons = allBeacons.ToArray();
    }

    void onUnregisterBeacon(Beacon beacon)
    {
        var file = beacon.file;
        if (beacons.ContainsKey(file)) {
            var curr = beacons[file];
            curr.Remove(beacon);

            if (curr.Count == 0) beacons.Remove(file);
            else beacons[file] = curr;
        }

        allBeacons.Remove(beacon);
        Save.beacons = allBeacons.ToArray();
    }

    void onDiscoveredBeacon(Beacon beacon)
    {
        var file = beacon.file;

        Discovery.DiscoverFile(file);
        Nest.AddBeacon(beacon);
    }

    #endregion

    #region Nest callbacks

    void onNestStateChanged()
    {
        Debug.LogFormat("Nest state changed to --> {0}", Nest.open);
        Save.nestOpen = Nest.open; // Set save state in file
    }

    void onIngestBeacon(Beacon beacon)
    {
        beacon.visible = false;
        Save.beacons = allBeacons.ToArray();

        if (beacon.type == Beacon.Type.None) return;

        var file = beacon.file;
        var tex = Library.GetTexture(file);

        if (tex != null) {
            Debug.Log("Load in LIBRARY");
            Quilt.Add(tex); // Load texture from library
        }
        else {
            Debug.Log("Load from LIBRARY");
            Quilt.Push(file); // Load texture in quilt
        }
    }

    void onReleaseBeacon(Beacon beacon)
    {
        beacon.visible = true;
        Save.beacons = allBeacons.ToArray();

        if (beacon.type == Beacon.Type.None) return;

        var file = beacon.file;
        Quilt.Pop(file);
    }

    #endregion

    #region Wizard callbacks



    #endregion

    #region Quilt callbacks

    void onDisposeQuiltTexture(Texture texture)
    {

    }

	#endregion
}
