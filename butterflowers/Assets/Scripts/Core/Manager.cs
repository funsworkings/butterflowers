using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = SimpleFileBrowser.FileSystemEntry;
using System.Runtime.InteropServices;

public class Manager : Spawner
{
    public static Manager Instance = null;

    #region External

    [SerializeField] Settings.WorldPreset Preset;

    FileNavigator Files = null;
    Discovery Discovery = null;
    Nest Nest = null;
    Quilt Quilt = null;

	#endregion

	#region Attributes

	[SerializeField] int maxBeacons = 100;

    #endregion

    #region Collections

    Dictionary<string, Beacon> beacons = new Dictionary<string, Beacon>();

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

        Discovery = Discovery.Instance;
        Discovery.Preset = Preset;

        Files = FileNavigator.Instance;
        Nest = Nest.Instance;
        Quilt = Quilt.Instance;

        Discovery.onLoad += Initialize;
    }

    protected override void OnDestroy(){
        base.OnDestroy();

        Discovery.onLoad -= Initialize;

        //Sun.onCycle -= RefreshBeacons;

        Nest.onAddBeacon -= onIngestBeacon;
        Nest.onRemoveBeacon -= onReleaseBeacon;

        Beacon.Discovered -= onDiscoveredBeacon;
        Beacon.Destroyed -= onDestroyedBeacon;

        Files.onRefresh -= RefreshBeacons;
    }

    #endregion

    #region Internal

    void Initialize()
    {
        RefreshBeacons();

        //Sun.onCycle += RefreshBeacons;

        Nest.onAddBeacon += onIngestBeacon;
        Nest.onRemoveBeacon += onReleaseBeacon;

        Beacon.Discovered += onDiscoveredBeacon;
        Beacon.Destroyed += onDestroyedBeacon;

        Files.onRefresh += RefreshBeacons;
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

    // If beacon is in subset, IGNORE
    // If beacon is not in subset, DELETE

    void RefreshBeacons()
    {
        var files = Files.GetFiles();
        var subset = files.PickRandomSubset<FileSystemEntry>(maxBeacons).ToList(); // Target beacons        

        var current = (beacons != null)? beacons.Keys.ToList(): new List<string>();
        if (current.Count > 0) {

            var target = Files.GetPathsFromFiles(subset.ToArray());

            for (int i = 0; i < current.Count; i++) 
            {
                var path = current[i];
                if (!target.Contains(current[i])) {
                    var beacon = beacons[path];

                    beacon.Delete(); // Remove inactive beacon
                    beacons.Remove(path);
                }
            }
        }

        CreateBeaconsFromFiles(subset.ToArray()); // Instantiate all target files
    }

    void CreateBeaconsFromFiles(FileSystemEntry[] files)
    {
        int size = files.Length;
        int capacity = instances.Count;

        // Debug.LogFormat("Attempt to create {0} beacons", size);

        GameObject instance = null;
        Beacon beaconInstance = null;
        for (int i = 0; i < files.Length; i++) {
            var file = files[i];
            var path = file.Path;

            bool exists = (beacons.ContainsKey(path));
            if (!exists) {
                instance = InstantiatePrefab(); // Create new beacon prefab

                beaconInstance = instance.GetComponent<Beacon>();
                beaconInstance.file = path;
                beaconInstance.fileEntry = file;
                beaconInstance.origin = instance.transform.position;

                //Debug.LogFormat("Discovered {0}? {1}", path, Discovery.HasDiscoveredFile(path));
                var discovered = Discovery.HasDiscoveredFile(path);
                beaconInstance.discovered = discovered; // Set if beacon has been discovered

                beaconInstance.Appear();

                beacons.Add(path, beaconInstance);
            }
        }
    }

    void ClearBeacons(){
        beacons = new Dictionary<string, Beacon>();

        Quilt.Dispose();
        Nest.Dispose();

        Clear();
    }

    public bool ActivateRandomBeacon()
    {
        var beacons = this.beacons.Values;
        IEnumerable<Beacon> inactive = beacons.Where(beacon => !Nest.HasBeacon(beacon));

        int count = inactive.Count();
        if (count == 0) return false;

        var _beacon = inactive.ElementAt(Random.Range(0, count));
        _beacon.Discover();

        return true;
    }

    #endregion

    #region Beacon callbacks

    void onDiscoveredBeacon(Beacon beacon)
    {
        var file = beacon.file;

        Discovery.DiscoverFile(file);
        Nest.AddBeacon(beacon);
    }

    void onDestroyedBeacon(Beacon beacon)
    {
        var file = beacon.file;
        if (beacons.ContainsKey(file))
            beacons.Remove(file);

        Quilt.Pop(file); // Attempt remove from quilt if doesn't exist
    }

    #endregion

    #region Nest callbacks

    void onIngestBeacon(Beacon beacon)
    {
        var file = beacon.file;
        Quilt.Push(file);
    }

    void onReleaseBeacon(Beacon beacon)
    {
        var file = beacon.file;
        Quilt.Pop(file);
    }

	#endregion
}
