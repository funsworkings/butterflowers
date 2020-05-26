using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    protected List<GameObject> instances = new List<GameObject>();

    [SerializeField] int count = 0;
    
    [SerializeField] protected Transform root = null;
    [SerializeField] protected int amount = 100;
    [SerializeField] protected bool spawnOnAwake = true;
    [SerializeField] protected bool continuous = false;

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

    [SerializeField] protected Vector3 boundsOffset = Vector3.zero, boundsMultiplier = Vector3.one;

    protected virtual void Awake() {
        if(root == null)
            root = transform;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        CalculateBounds();
        //Debug.LogFormat("center: {0} extents:{1}", m_center, m_extents);

        if(continuous)
            StartCoroutine("SpawnContinously");
        else {
            if (spawnOnAwake)
                Spawn(amount);
        }
    }

    protected virtual void Update(){ count = instances.Count; }

    protected virtual void OnDestroy(){
        if(continuous)
            StopCoroutine("SpawnContinously");
    }

    public GameObject[] Spawn(int amount = 0){
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

    public GameObject Spawn(Vector3 position, Quaternion rotation, GameObject inst = null)
    { 
        if (prefab == null) return null;

        var instance = InstantiatePrefab(null);

        instance.transform.position = position;
        instance.transform.rotation = rotation;

        return instance;
    }

    public bool Despawn(int amount = 0){
        int count = instances.Count;
        if(count == 0 || amount == 0)
            return false;

        if(amount > count)
            amount = count; // Ensure not greater to current instance count

        for(int i = 0; i < amount; i++){
            if(instances[0] != null){
                GameObject.Destroy(instances[0]);
                instances.RemoveAt(0);
            }
        }

        return true;
    }
    
    public bool Clear(){
        int count = instances.Count;
        return Despawn(count);
    }

    IEnumerator SpawnContinously(){
        while(true){
            int current = instances.Count;

            if(current < amount)
                Spawn((amount - current));
            else if(current > amount)
                Despawn((current - amount));

            yield return null;
        }
    }

    protected virtual void DecidePosition(ref Vector3 pos)
    {
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        float bx = boundsMultiplier.x, by = boundsMultiplier.y, bz = boundsMultiplier.z;
      
        offset = m_center + (new Vector3(Random.Range(-extents.x, extents.x)*bx,
                                         Random.Range(-extents.y, extents.y)*by,
                                         Random.Range(-extents.z, extents.z)*bz));

        position = root.TransformPoint(offset) + boundsOffset;
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

        instance.transform.parent = root;
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
        Collider col = root.GetComponent<Collider>();

        m_center = col.bounds.center;
        m_extents = col.bounds.extents;

        col.enabled = false; // Disable collider after fetching center+bounds
    }
}
