using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using FileSystemEntry = SimpleFileBrowser.FileSystemEntry;

public class Room : Spawner
{
    public static Room Instance = null;

    #region External

    [SerializeField] Settings.WorldPreset Preset;

    Discovery Discovery = null;
    FileNavigator Files = null;
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

    void Initialize()
    {
        SpawnBeacons();

        Beacon.Discovered += onDiscoveredBeacon;
        Files.onRefresh += SpawnBeacons;
    }

    protected override void OnDestroy(){
        base.OnDestroy();

        Discovery.onLoad -= Initialize;

        Beacon.Discovered -= onDiscoveredBeacon;
        Files.onRefresh -= SpawnBeacons;
    }

    #endregion

    #region Spawner overrides

    protected override void CalculateBounds()
    {
        m_center = Vector3.zero;
        m_extents = GetComponent<MeshFilter>().mesh.bounds.extents;
    }

    protected override void DecideRotation(ref Quaternion rot)
    {
        rot = transform.rotation;
    }

    #endregion

    #region Beacon operations

    void SpawnBeacons(){
        ClearBeacons();

        var files = FileNavigator.Instance.GetFiles();
        var subset = files.PickRandomSubset<FileSystemEntry>(maxBeacons).ToArray();

        CreateBeaconsFromFiles(subset);
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
                if (capacity >= size)
                    instance = InstantiatePrefab(instances[i]);
                else
                    instance = InstantiatePrefab();

                beaconInstance = instance.GetComponent<Beacon>();
                beaconInstance.file = path;
                beaconInstance.fileEntry = file;
                beaconInstance.origin = instance.transform.position;

                Debug.LogFormat("Discovered {0}? {1}", path, Discovery.HasDiscoveredFile(path));
                var discovered = Discovery.HasDiscoveredFile(path);
                beaconInstance.discovered = discovered; // Set if beacon has been discovered

                beacons.Add(path, beaconInstance);
            }
        }
    }

    void ClearBeacons(){
        beacons = new Dictionary<string, Beacon>();
        Clear();
    }

    #endregion

    #region Beacon callbacks

    void onDiscoveredBeacon(Beacon beacon)
    {
        var file = beacon.file;

        Discovery.DiscoverFile(file);
        Quilt.LoadTexture(file);

        beacon.Warp(Nest.transform.position);
    }

    void onDestroyedBeacon(Beacon beacon)
    {
        var file = beacon.file;
        if (beacons.ContainsKey(file))
            beacons.Remove(file);
    }

    #endregion

    #region Deprecated

    /*

    beaconInstance.thumbnail = Helpers.GenerateThumbnailFromPath(paths[i]);

    */

    #endregion
}
