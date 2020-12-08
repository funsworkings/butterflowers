using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using B83.Win32;
using Type = Beacon.Type;
using Locale = Beacon.Locale;
using Settings;
using uwu;
using uwu.Extensions;
using uwu.Gameplay;
using Object = System.Object;

public class BeaconManager : Spawner, IReactToSunCycle, ISaveable
{
    public static BeaconManager Instance;

	#region Events

	public System.Action onUpdateBeacons;
    public System.Action<Beacon> onPlantBeacon;

	#endregion

	#region External

	World World;
    Nest Nest;
    GameDataSaveSystem SaveSys;
    Library Library;

    #endregion

    #region Properties

    [SerializeField] WorldPreset preset;

	[SerializeField] Transform m_beaconInfoContainer = null;

    #endregion

    #region Attributes

    [SerializeField] LayerMask beaconGroundMask;
    [SerializeField] int maxBeacons = 100;

	#endregion

	#region Collections

	Dictionary<string, List<Beacon>> beacons = new Dictionary<string, List<Beacon>>();
	List<Beacon> allBeacons = new List<Beacon>();

	#endregion

	#region Accessors

	public Beacon[] AllBeacons => allBeacons.ToArray();

	public Beacon[] ActiveBeacons => allBeacons.Where(beacon => Nest.HasBeacon(beacon)).ToArray();
	public Beacon[] InactiveBeacons => allBeacons.Where(beacon => !Nest.HasBeacon(beacon)).ToArray();
	public Beacon[] PlantedBeacons => allBeacons.Where(beacon => beacon.state == Locale.Planted).ToArray();
	public Beacon[] LiveBeacons => allBeacons.Where(beacon => (beacon.state != Locale.Planted || beacon.state != Locale.Destroyed)).ToArray();

	public Transform BeaconInfoContainer => m_beaconInfoContainer;

	public Beacon GetBeaconByFile(string file)
	{
		List<Beacon> b = null;
		beacons.TryGetValue(file, out b);

		if (b != null && b.Count > 0)
			return b[0];

		return null;
	}

    #endregion

    #region Monobehaviour callbacks

    protected override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    void OnEnable()
	{
		Beacon.OnRegister += onRegisterBeacon;
		Beacon.OnUnregister += onUnregisterBeacon;

		Beacon.Activated += onActivatedBeacon;
        Beacon.Planted += onPlantedBeacon;
        
        Library = Library.Instance;
	        Library.onDeletedFiles += UserDeletedFiles;
	        Library.onRecoverFiles += UserRecoveredFiles;
	}

	void OnDisable()
	{
		Beacon.OnRegister -= onRegisterBeacon;
		Beacon.OnUnregister -= onUnregisterBeacon;

		Beacon.Activated -= onActivatedBeacon;
        Beacon.Planted -= onPlantedBeacon;
        
        Library.onDeletedFiles -= UserDeletedFiles;
        Library.onRecoverFiles -= UserRecoveredFiles;
	}

    #endregion

    #region Cycle

    public void Cycle(bool refresh)
    {
		WipeBeacons(); // Clear out flammable beacons
    }
    
    #endregion

    #region Spawner overrides

    protected override void CalculateBounds()
    {
        m_center = Vector3.zero;
        m_extents = root.GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    public override void DecidePosition(ref Vector3 pos)
    {
        base.DecidePosition(ref pos); // Get initial ray position

        Vector3 origin = pos + root.up * 9f;
        Vector3 dir = -root.up;

        Ray ray = new Ray(origin, dir);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, 999f, beaconGroundMask.value)) 
        {
            pos = hit.point;
        }
    }

    public override void DecideRotation(ref Quaternion rot)
    {
        rot = transform.rotation;
    }

    #endregion

    #region Operations

    public Beacon CreateBeacon(string path, Type type, Locale state, Vector3 position, bool usePosition = false, bool load= true)
    {
        var instance = InstantiatePrefab(); // Create new beacon prefab
        if (usePosition) 
	        instance.transform.position = position;

        var origin = instance.transform.position;
        var discovered = Library.HasDiscoveredFile(path); // Check if seen before

        var beacon = instance.GetComponent<Beacon>();
            beacon.file = path;
            beacon.fileEntry = null;
            beacon.discovered = discovered; // Set if beacon has been discovered
            
        Debug.Log("Add beacon => " + path);
        
        beacon.Initialize(type, state, origin, BeaconInfoContainer, load);

        Events.ReceiveEvent(EVENTCODE.BEACONADD, AGENT.World, AGENT.Beacon, details: beacon.file);

        return beacon;
    }

    public void CreateBeaconInstance(string path, Type type, Vector3 point, bool usePosition)
    {
	    CreateBeacon(path, type, Locale.Terrain, point, usePosition: usePosition, load:false);
    }

    public void DeleteBeacon(Beacon beacon, bool overridenest = false)
    {
        bool success = !(beacon.state == Locale.Planted);

        if (success) {
	        if (overridenest)
		        beacon.Delete();
	        else 
	        {
		        if (!Nest.HasBeacon(beacon)) // Don't delete if currently inside nest
			        beacon.Delete();
		        else
			        success = false;
	        }
        }

        if (success)
            Events.ReceiveEvent(EVENTCODE.BEACONDELETE, AGENT.World, AGENT.Beacon, details: beacon.file);
    }

    // If beacon is in subset, IGNORE
    // If beacon is not in subset, DELETE

    public void RefreshBeacons()
    {
	    //DeleteDeprecatedBeacons(lib);

        var desktop = Library.UserFiles;
        var enviro = Library.WorldFiles;

        var files = desktop.Concat(enviro);
        var blacklist = PlantedBeacons.Select(b => b.file);

        files = files.Except(blacklist); // Ignore items from blacklist

        var composite = files.ToArray();
        var subset = composite.PickRandomSubset<string>(maxBeacons).ToList(); // Random subset from aggregate collection    

        var current = (beacons != null) ? beacons.Keys.ToList() : new List<string>();
        if (current.Count > 0) {

            var target = subset;
            for (int i = 0; i < current.Count; i++) 
            {
                var path = current[i];

                if (!target.Contains(current[i])) {

                    var bs = beacons[path].ToArray();
                    for (int j = 0; j < bs.Length; j++) 
                    {
                        var b = bs[j];
                        DeleteBeacon(b);
                    }

                }
            }

        }
        
        desktop = desktop.Intersect(subset).ToArray();
        enviro = enviro.Intersect(subset).ToArray();
        
        CreateBeacons(desktop, Type.Desktop, Locale.Terrain);
        CreateBeacons(enviro, Type.External, Locale.Terrain);
    }

    public void RestoreBeacons(BeaconData[] data, bool deprecate = false)
    {
        Debug.LogFormat("Recovered {0} beacons from save!", data.Length);

        if (data == null || data.Length == 0) // Ignore restore if data is empty
        {
	        RefreshBeacons();
	        return;
        }

        for (int i = 0; i < data.Length; i++) {
            var beacon = data[i];

            string p = beacon.path;
            Vector3 loc = new Vector3(beacon.x, beacon.y, beacon.z);
            Type t = (Type)beacon.type;
            Locale s = (Locale)beacon.state;

            var instance = CreateBeacon(p, t, s, loc, usePosition:true);
            if (instance == null)
                continue; // Bypass null beacon

            if (s == Locale.Nest) 
            {
                onActivatedBeacon(instance); // Send to nest if immediately found
                Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.World, AGENT.Beacon, details: instance.file);
            }
        }

        if (deprecate) 
        {
           // DeleteDeprecatedBeacons(lib);
        }
    }

    public void WipeBeacons()
    {
	    foreach (KeyValuePair<string, List<Beacon>> lookup in beacons) 
	    {
		    var file = lookup.Key;
		    var beacons = lookup.Value.ToArray();

		    foreach (Beacon b in beacons) 
		    {
				if(b.IsOnFire) DeleteBeacon(b, overridenest:true);    
		    }
	    }
    }

    public void CreateBeacons(string[] files, Type type, Locale state)
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

            if (!exists)
                CreateBeacon(files[i], type, state, Vector3.zero);
        }
    }

    public void DeleteDeprecatedBeacons(Library lib)
    {
        var current = (beacons != null) ? beacons.Keys.ToList() : new List<string>();

        var path = "";
        for (int i = 0; i < current.Count; i++) {
            path = current[i];

            if (!lib.ContainsFile(path)) {
                var bs = beacons[path].ToArray();
                foreach (Beacon b in bs)
                    DeleteBeacon(b, true);
            }
        }
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
		if (onUpdateBeacons != null)
			onUpdateBeacons();
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
		if (onUpdateBeacons != null)
			onUpdateBeacons();
	}

	void onActivatedBeacon(Beacon beacon)
	{
		Nest.AddBeacon(beacon);
	}

    void onPlantedBeacon(Beacon beacon)
    {
	    if (onPlantBeacon != null)
            onPlantBeacon(beacon);
    }

	#endregion
	
	#region Library callbacks

	void UserDeletedFiles(string[] files)
	{
		foreach (string file in files) 
		{
			if (beacons.ContainsKey(file)) 
			{
				var list = beacons[file];
				foreach(Beacon b in list)
					b.Destroy();	
			}	
		}
	}

	void UserRecoveredFiles(string[] files)
	{
		foreach (string file in files) 
		{
			if (beacons.ContainsKey(file)) 
			{
				var list = beacons[file];
				foreach(Beacon b in list)
					b.Recover();
			}	
		}
	}
	
	#endregion
	
	#region Save/load

	public Object Save()
	{
		return allBeacons.ToArray();
	}

	public void Load(Object dat)
	{
		maxBeacons = preset.amountOfBeacons; // Initialize max beacons

		World = World.Instance;
		Nest = Nest.Instance;
		SaveSys = GameDataSaveSystem.Instance;
		Library = Library.Instance;

		if (dat != null) 
		{
			var beacons = (BeaconData[]) dat;
			RestoreBeacons(beacons);
		}
	}

	#endregion
}
