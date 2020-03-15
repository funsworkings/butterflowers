using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Spawner : MonoBehaviour
{
    public GameObject butterflyPrefab;
    
    [SerializeField] int amount = 100;
    [SerializeField] new Camera camera;

    Collider bounds;


    public Vector3 center {
        get{
            return transform.position + bounds.bounds.center;
        }
    }
    
    public Vector3 extents {
        get{
            return bounds.bounds.extents;
        }
    }

    void Awake() {
        bounds = GetComponent<Collider>();    
    }

    // Start is called before the first frame update
    void Start()
    {
        Butterfly.Died += onRespawnButterfly;

        if (butterflyPrefab != null)
            Spawn();
    }

    void Spawn(){
        GameObject instance = null;
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        for(int i = 0; i < amount; i++)
        {
            InstantiateButterfly();
        }
    }

    void DecidePosition(ref Vector3 pos, ref Quaternion rot)
    {
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        if (camera == null)
        {
            offset = new Vector3(Random.Range(-extents.x, extents.x),
                                Random.Range(-extents.y, extents.y),
                                Random.Range(-extents.z, extents.z));

            position = transform.TransformPoint(offset);
        }
        else
        {
            offset = new Vector3(Random.Range(0f, 1f),
                                 Random.Range(0f, 1f),
                                 Random.Range(extents.z, 2f * extents.z));

            position = camera.ViewportToWorldPoint(offset);
        }

        pos = position;
        rot = butterflyPrefab.transform.rotation;
    }

    void InstantiateButterfly(Butterfly butterfly = null)
    {
        var instance = (butterfly == null)? null:butterfly.gameObject;
        if (instance == null)
            instance = Instantiate(butterflyPrefab, transform);


        Vector3 pos = Vector3.zero;
        Quaternion rot = transform.rotation;

        DecidePosition(ref pos, ref rot);

        instance.transform.position = pos;
        instance.transform.rotation = rot;
    }

    void OnDestroy() {
        Butterfly.Died -= onRespawnButterfly;
    }

    void onRespawnButterfly(Butterfly butterfly)
    {
        InstantiateButterfly(butterfly);
        butterfly.Reset();
    }
}
