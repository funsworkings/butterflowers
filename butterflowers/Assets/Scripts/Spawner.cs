using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    
    [SerializeField] int amount = 100;

    Collider bounds;


    Vector3 m_center = Vector3.zero;
    public Vector3 center {
        get{
            return transform.position + m_center;
        }
    }

    Vector3 m_extents = Vector3.zero;
    public Vector3 extents {
        get{
            return m_extents;
        }
    }

    void Awake() {
        bounds = GetComponent<Collider>();    
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_center = bounds.bounds.center;
        m_extents = bounds.bounds.extents;

        bounds.enabled = false; // Disable collider after fetching center+bounds

        if (prefab != null)
            Spawn();
    }

    void Spawn(){
        GameObject instance = null;
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        for(int i = 0; i < amount; i++)
        {
            InstantiatePrefab();
        }
    }

    protected virtual void DecidePosition(ref Vector3 pos)
    {
        Vector3 offset = Vector3.zero, position = Vector3.zero;

      
        offset = new Vector3(Random.Range(-extents.x, extents.x),
                             Random.Range(-extents.y, extents.y),
                             Random.Range(-extents.z, extents.z));

        position = transform.TransformPoint(offset);


        pos = position;
    }

    protected virtual void DecideRotation(ref Quaternion rot)
    {
        rot = prefab.transform.rotation;
    }

    protected void InstantiatePrefab(GameObject inst = null)
    {
        var instance = inst;
        if (instance == null)
            instance = Instantiate(prefab, transform);

        Vector3 pos = Vector3.zero;
        Quaternion rot = transform.rotation;

        DecidePosition(ref pos);
        DecideRotation(ref rot);

        instance.transform.position = pos;
        instance.transform.rotation = rot;
    }
}
