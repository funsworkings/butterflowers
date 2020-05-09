using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Room : Spawner
{

    #region Attributes

    [SerializeField] int maxBeacons = 100;

    #endregion

    #region Monobehaviour callbacks

    protected override void Awake()
    {
        base.Awake();

        spawnOnAwake = false;
    }

    void OnEnable()
    {
        Navigator.onRefreshFilesMatchingFilterInDirectory += FilterThumbnails;
    }

    void OnDisable()
    {
        Navigator.onRefreshFilesMatchingFilterInDirectory -= FilterThumbnails;
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

    void FilterThumbnails(string[] paths)
    {
        string[] selection = paths.PickRandomSubset<string>(maxBeacons).ToArray();
        CreateInstancesFromThumbnails(selection);
    }

    void CreateInstancesFromThumbnails(string[] paths)
    {
        int size = paths.Length;
        int capacity = instances.Count;

        GameObject instance = null;
        Beacon beaconInstance = null;
        for (int i = 0; i < paths.Length; i++)
        {
            if (capacity >= size)
                instance = InstantiatePrefab(instances[i]);
            else
                instance = InstantiatePrefab();

            beaconInstance = instance.GetComponent<Beacon>();
            beaconInstance.file = paths[i];
            beaconInstance.thumbnail = Helpers.GenerateThumbnailFromPath(paths[i]);
        }
    }
}
