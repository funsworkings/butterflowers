using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = SimpleFileBrowser.FileSystemEntry;
using Memory = Wizard.Memory;
using System.Runtime.InteropServices;

using UnityEngine.Events;

public class World : Spawner
{
    public static World Instance = null;

    public static bool LOAD = false;

    #region Events

    public UnityEvent onDiscovery;

    public System.Action onRefreshBeacons;

	#endregion



	#region External

	[SerializeField] Settings.WorldPreset Preset;
    [SerializeField] Wizard.Memories Memories;
    [SerializeField] CameraManager CameraManager;

    GameDataSaveSystem Save = null;
    Library Library = null;
    FileNavigator Files = null;
    Scribe Scribe = null;
    Discovery Discovery = null;
    MotherOfButterflies Butterflowers = null;
    Sun Sun = null;
    Nest Nest = null;
    Quilt Quilt = null;
    [SerializeField] Wizard.Controller Wizard = null;
    Loading Loader = null;

	#endregion

	#region Attributes

	[SerializeField] int maxBeacons = 100;

    public Texture2D DEFAULT_NULL_TEXTURE;

    [SerializeField] Spawner positiveSpawner;
    [SerializeField] GameObject pr_PositivePS;

    #endregion

    #region Properties

    [SerializeField] Transform m_beaconInfoContainer = null;

    [SerializeField] Snapshot WorldCamera;
    [SerializeField] Camera previousMainCamera = null;

	#endregion

	#region Collections

	Dictionary<string, List<Beacon>> beacons = new Dictionary<string, List<Beacon>>();
    List<Beacon> allBeacons = new List<Beacon>();

    [SerializeField] string[] beaconPaths = new string[] { };

    [SerializeField] List<int> wizardDiscoveries = new List<int>();

    #endregion



    #region Accessors

    public Beacon[] AllBeacons => allBeacons.ToArray();

    public Beacon[] ActiveBeacons   => allBeacons.Where(beacon => Nest.HasBeacon(beacon)).ToArray();
    public Beacon[] InactiveBeacons => allBeacons.Where(beacon => !Nest.HasBeacon(beacon)).ToArray();

    public Transform BeaconInfoContainer => m_beaconInfoContainer;

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
        Library.WizardFiles = Memories;

        Discovery = Discovery.Instance;
        Discovery.Preset = Preset;

        Files = FileNavigator.Instance;
        Scribe = Scribe.Instance;
        Sun = Sun.Instance;
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Butterflowers = FindObjectOfType<MotherOfButterflies>();

        Loader = FindObjectOfType<Loading>();

        StartCoroutine("Initialize");
    }

    protected override void Update()
    {
        base.Update();

        beaconPaths = allBeacons.Select(beacon => beacon.file).ToArray();

        if (Input.GetKeyDown(KeyCode.W))
            PositiveBurst();
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

        Nest.RestoreCapacity(Save.nestcapacity);

        while (!Discovery.load) yield return null;

        SubscribeToEvents();

        var dat = Save.beaconData; Debug.LogFormat("Found {0} beacons from SAVE", dat.Length);
        var refresh = (dat == null || dat.Length == 0) || !Preset.persistBeacons;

        Library.Initialize(refresh);

        while (Library.loadprogress < 1f) 
        {
            Loader.progress = Library.loadprogress;
            yield return null;
        }
        Loader.progress = 1f;

        Scribe.Restore(Save.logs);

        wizardDiscoveries = Discovery.discoveries.Intersect(Library.wizard_files.Select(w => Library.ALL_FILES.IndexOf(w))).ToList();

        RestoreNest();

        if (!refresh) {
            RestoreBeacons(dat); // Restore from save file
            DeleteDeprecatedBeacons();

            if (onRefreshBeacons != null) // Fire event during initialization
                onRefreshBeacons();
        }
        else
            RefreshBeacons();

        Sun.active = true; // Set sun into motion
        LOAD = true;
    }

    void SubscribeToEvents()
    {
        Sun.onCycle += Advance;

        Discovery.onDiscover += onDiscoveredSomethingNew;

        Nest.onOpen.AddListener(onNestStateChanged);
        Nest.onClose.AddListener(onNestStateChanged);
        Nest.onAddBeacon += onIngestBeacon;
        Nest.onRemoveBeacon += onReleaseBeacon;

        Beacon.OnRegister += onRegisterBeacon;
        Beacon.OnUnregister += onUnregisterBeacon;
        Beacon.Discovered += onDiscoveredBeacon;

        Wizard.onDiscoverMemory += onDiscoveredMemory;

        Quilt.onDisposeTexture += onDisposeQuiltTexture;

        Library.OnAddItem += onAddLibraryItem;
    }

    void UnsubscribeToEvents()
    {
        Sun.onCycle -= Advance;

        Discovery.onDiscover -= onDiscoveredSomethingNew;

        Nest.onOpen.RemoveListener(onNestStateChanged);
        Nest.onClose.RemoveListener(onNestStateChanged);
        Nest.onAddBeacon -= onIngestBeacon;
        Nest.onRemoveBeacon -= onReleaseBeacon;

        Beacon.OnRegister -= onRegisterBeacon;
        Beacon.OnUnregister -= onUnregisterBeacon;
        Beacon.Discovered -= onDiscoveredBeacon;

        Wizard.onDiscoverMemory -= onDiscoveredMemory;

        Quilt.onDisposeTexture -= onDisposeQuiltTexture;

        Library.OnAddItem -= onAddLibraryItem;
    }

    #endregion

    #region Spawner overrides

    protected override void CalculateBounds()
    {
        m_center = Vector3.zero;
        m_extents = root.GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    public override void DecideRotation(ref Quaternion rot)
    {
        rot = transform.rotation;
    }

    #endregion

    #region Sun callbacks

    bool photo = false;

    void Advance()
    {
        StartCoroutine("Advancing");
    }

    IEnumerator Advancing()
    {
        Nest.Pulse();
        yield return new WaitForEndOfFrame();

        TakePicture();
        photo = true;

        yield return new WaitForEndOfFrame();
        while (photo)
            yield return null;

        Nest.AttemptUpdateCapacity();

        bool success = Nest.Close(); // Close nest
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
        if (!Library.ContainsFile(path)) return null;

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

        Events.ReceiveEvent(EVENTCODE.BEACONADD, AGENT.World, AGENT.Beacon, details: beacon.file);

        return beacon;
    }
    
    public void DeleteBeacon(Beacon beacon, bool overridenest = false)
    {
        bool success = true;

        if (overridenest) 
            beacon.Delete();
        else {
            if (!Nest.HasBeacon(beacon))
                beacon.Delete();
            else
                success = false;
        }

        if(success)
            Events.ReceiveEvent(EVENTCODE.BEACONDELETE, AGENT.World, AGENT.Beacon, details: beacon.file);
    }

    // If beacon is in subset, IGNORE
    // If beacon is not in subset, DELETE

    void RefreshBeacons()
    {
        if (Library.loadprogress < 1f) return; // Ignore refresh if library not finished

        DeleteDeprecatedBeacons();

        var desktop = Library.desktop_files;
        var wizard = Library.wizard_files.Where(file => Discovery.HasDiscoveredFile(file)); // Only choose 'discovered' wizard files
        var shared = Library.shared_files;

        var files = (desktop.Concat(wizard)).ToArray();
        var subset = files.PickRandomSubset<string>(maxBeacons).ToList(); // Random subset from aggregate collection    

        var current = (beacons != null)? beacons.Keys.ToList(): new List<string>();
        if (current.Count > 0) {

            var target = subset;
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

        wizard = wizard.Intersect(subset);
        desktop = desktop.Intersect(subset).ToArray();

        CreateBeacons(wizard.ToArray(), Beacon.Type.Wizard);
        CreateBeacons(desktop, Beacon.Type.Desktop);

        if (onRefreshBeacons != null)
            onRefreshBeacons();
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
            if (instance == null)
                continue; // Bypass null beacon

            if (!v) {
                onDiscoveredBeacon(instance); // Send to nest if immediately found
                Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.World, AGENT.Beacon, details: instance.file);

            }
        }
    }

    Beacon[] CreateBeacons(string[] files, Beacon.Type type, bool overrides = false)
    {
        List<Beacon> instances = new List<Beacon>();
        for (int i = 0; i < files.Length; i++) {
            var path = files[i];

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

            if (!exists || overrides) {
                var beacon = CreateBeacon(files[i], type);
                if (beacon != null) // Could not success instantiate
                    instances.Add(beacon);
            }
        }

        return instances.ToArray();
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

    public Beacon CreateBeaconForDesktop(string file)
    {
        string name = file;
        return CreateBeacon(name, Beacon.Type.Desktop);
    }

    public Beacon CreateBeaconForWizard(Texture2D texture)
    {
        string name = texture.name;
        return CreateBeacon(name, Beacon.Type.Wizard);
    }

    public Beacon FetchRandomBeacon(bool active = false)
    {
        var beacons = this.beacons.Values;
        IEnumerable<Beacon> collection = (active) ? ActiveBeacons : InactiveBeacons;

        int count = collection.Count();
        if (count == 0) return null;

        var _beacon = collection.ElementAt(Random.Range(0, count));
        return _beacon;
    }

    #endregion

    #region World operations

    public SUGGESTION[] GetSuggestions()
    {
        SUGGESTION suggestion = SUGGESTION.NULL;
        float max = -1f;

        suggestion = SUGGESTION.ENDOW_KINDNESS;
        max = (1f - Butterflowers.GetHealth());

        float nest = (1f - Nest.fill);
        if (nest > max) {
            suggestion = SUGGESTION.ADD_BEACON;
            max = nest;
        }

        float absorb = (1f - GetAbsorption());
        if (absorb > max) {
            suggestion = SUGGESTION.VISIT_TREE;
            max = nest;
        }

        return new SUGGESTION[] { suggestion };
    }

    #endregion

    #region Wizard operations


    public float GetAbsorption()
    {
        int current = wizardDiscoveries.Count;
        int total = Memories.items.Length;

        return (float)current / total;
    }

    #endregion

    #region Camera operations

    void TakePicture()
    {
        var camera = WorldCamera.camera;

        previousMainCamera = CameraManager.MainCamera;
        CameraManager.MainCamera = camera;
        camera.enabled = true;

        WorldCamera.onSuccess += onReceivePicture;

        StartCoroutine("TakingPicture");
    }

    IEnumerator TakingPicture()
    {
        yield return new WaitForSeconds(.167f);
        WorldCamera.Capture();
    }

    void onReceivePicture(Texture2D image)
    {
        var name = Extensions.RandomString(12);
        var camera = WorldCamera.camera;

        Library.SaveTexture(name, image);

        CameraManager.MainCamera = previousMainCamera;

        camera.enabled = false;
        previousMainCamera = null;

        WorldCamera.onSuccess -= onReceivePicture;

        photo = false;

        Events.ReceiveEvent(EVENTCODE.PHOTOGRAPH, AGENT.World, AGENT.Terrain);
    }

    #endregion

    #region Miscellaneous operations

    public void PositiveBurst()
    {
        Vector3 pos = Vector3.zero;
        positiveSpawner.DecidePosition(ref pos);

        Quaternion rot = Quaternion.identity;
        positiveSpawner.DecideRotation(ref rot);

        var instance = Instantiate(pr_PositivePS, pos, rot);
        var ps = instance.GetComponent<ParticleSystem>();

        ps.Play();
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

        bool success = Discovery.DiscoverFile(file);
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

    void onDiscoveredMemory(Memory memory)
    {
        var file = memory.name;
        bool success = Discovery.DiscoverFile(file);

        Debug.LogFormat("discovery = {0}", file);
        if (success) 
        {
            var beacon = CreateBeaconForWizard(memory.image);
            if (beacon != null) 
            {
                beacon.Discover();
            }
        }
    }

    #endregion

    #region Quilt callbacks

    void onDisposeQuiltTexture(Texture texture)
    {

    }

    #endregion

    #region Library callbacks

    void onAddLibraryItem(string item)
    {

    }

    #endregion

    #region Discovery callbacks

    void onDiscoveredSomethingNew(string file)
    {
        onDiscovery.Invoke();
        Events.ReceiveEvent(EVENTCODE.DISCOVERY, AGENT.Inhabitants, AGENT.Beacon, details:file);

        if (Library.IsWizard(file)) 
        {
            int index = Library.ALL_FILES.IndexOf(file);
            if (!wizardDiscoveries.Contains(index))
                wizardDiscoveries.Add(index);
        }
    }

	#endregion
}
