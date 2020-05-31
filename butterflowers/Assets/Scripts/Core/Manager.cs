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

    Library Library = null;
    FileNavigator Files = null;
    Discovery Discovery = null;
    Nest Nest = null;
    Quilt Quilt = null;
    Wizard.Controller Wizard = null;

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
    }

    protected override void Start(){
        base.Start();

        Library = Library.Instance;
        Library.WizardFiles = WizardMemoryBank;

        Discovery = Discovery.Instance;
        Discovery.Preset = Preset;

        Files = FileNavigator.Instance;
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Wizard = FindObjectOfType<Wizard.Controller>();

        Discovery.onLoad += Initialize;
    }

    protected override void OnDestroy(){
        base.OnDestroy();

        UnsubscribeToEvents();
    }

    #endregion

    #region Internal

    void Initialize()
    {
        Discovery.onLoad -= Initialize;

        SubscribeToEvents();
    }

    void SubscribeToEvents()
    {
        //Sun.onCycle += RefreshBeacons;
        Library.OnRefreshItems += RefreshBeacons;

        Nest.onAddBeacon += onIngestBeacon;
        Nest.onRemoveBeacon += onReleaseBeacon;

        Beacon.OnRegister += onRegisterBeacon;
        Beacon.OnUnregister += onUnregisterBeacon;
        Beacon.Discovered += onDiscoveredBeacon;

        Quilt.onDisposeTexture += onDisposeQuiltTexture;

        Library.Initialize();
    }

    void UnsubscribeToEvents()
    {
        //Sun.onCycle -= RefreshBeacons;
        Library.OnRefreshItems -= RefreshBeacons;

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

    #region Beacon operations

    public Beacon CreateBeacon(string path, Beacon.Type type = Beacon.Type.Desktop)
    {
        var instance = InstantiatePrefab(); // Create new beacon prefab

        var beacon = instance.GetComponent<Beacon>();
        beacon.file = path;
        beacon.origin = instance.transform.position;
        beacon.type = type;

        var discovered = Discovery.HasDiscoveredFile(path);
        beacon.discovered = discovered; // Set if beacon has been discovered

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
    }

    void onDiscoveredBeacon(Beacon beacon)
    {
        var file = beacon.file;

        Discovery.DiscoverFile(file);
        Nest.AddBeacon(beacon);
    }

    #endregion

    #region Nest callbacks

    void onIngestBeacon(Beacon beacon)
    {
        if (beacon.type == Beacon.Type.None) return;

        var file = beacon.file;

        if (beacon.type == Beacon.Type.Desktop) {
            Quilt.Push(file);
        }
        else {
            var mem = WizardMemoryBank.GetMemoryByName(beacon.file);
            if(mem != null) Quilt.Add(mem.image);
        }
    }

    void onReleaseBeacon(Beacon beacon)
    {
        if(beacon.type == Beacon.Type.None) return;

        var file = beacon.file;
        Quilt.Pop(file);
    }

    #endregion

    #region Wizard callbacks



    #endregion

    #region Quilt callbacks

    void onDisposeQuiltTexture(Texture texture)
    {
        string file = texture.name;
        if (Library.IsDesktop(file))
            Texture2D.Destroy(texture);
    }

	#endregion
}
