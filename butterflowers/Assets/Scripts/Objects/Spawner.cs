using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    protected List<GameObject> instances = new List<GameObject>();
    
    [SerializeField] protected int amount = 100;
    [SerializeField] protected bool spawnOnAwake = true;


    protected Vector3 m_center = Vector3.zero;
    public Vector3 center {
        get{
            return transform.position + m_center;
        }
    }

    protected Vector3 m_extents = Vector3.zero;
    public Vector3 extents {
        get{
            return m_extents;
        }
    }

    protected virtual void Awake() {

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        CalculateBounds();
        Debug.LogFormat("center: {0} extents:{1}", m_center, m_extents);

        if (spawnOnAwake)
            Spawn(amount);
    }

    protected GameObject[] Spawn(int amount = 0){
        if (prefab == null || amount == 0)
            return null;

        List<GameObject> spawned = new List<GameObject>();
        for(int i = 0; i < amount; i++)
        {
            var instance = InstantiatePrefab();
            spawned.Add(instance);
        }

        return spawned.ToArray();
    }

    protected virtual void DecidePosition(ref Vector3 pos)
    {
        Vector3 offset = Vector3.zero, position = Vector3.zero;

      
        offset = center + new Vector3(Random.Range(-extents.x, extents.x),
                                      Random.Range(-extents.y, extents.y),
                                      Random.Range(-extents.z, extents.z));

        position = transform.TransformPoint(offset);
        pos = position;
    }

    protected virtual void DecideRotation(ref Quaternion rot)
    {
        rot = prefab.transform.rotation;
    }

    protected GameObject InstantiatePrefab(GameObject inst = null)
    {
        bool refresh = true;

        var instance = inst;
        if (instance == null)
        {
            refresh = false;
            instance = Instantiate(prefab);
            instances.Add(instance);
        }

        Vector3 pos = Vector3.zero;
        Quaternion rot = transform.rotation;

        DecidePosition(ref pos);
        DecideRotation(ref rot);

        SetPrefabAttributes(instance, pos, rot);
        onInstantiatePrefab(instance, refresh);

        instance.transform.parent = transform;
        return instance;
    }

    protected virtual void SetPrefabAttributes(GameObject instance, Vector3 position, Quaternion rotation)
    {
        instance.transform.position = position;
        instance.transform.rotation = rotation;
    }

    protected virtual void onInstantiatePrefab(GameObject obj, bool refresh){}

    protected virtual void CalculateBounds()
    {
        Collider col = GetComponent<Collider>();

        m_center = col.bounds.center;
        m_extents = col.bounds.extents;

        col.enabled = false; // Disable collider after fetching center+bounds
    }
}
