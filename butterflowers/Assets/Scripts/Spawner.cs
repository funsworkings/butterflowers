using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    
    [SerializeField] int amount = 100;

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
        if(prefab != null)
            StartCoroutine("Spawn");
    }

    IEnumerator Spawn(){
        GameObject instance = null;
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        while(transform.childCount < amount){
            offset = new Vector3(Random.Range(-extents.x, extents.x),
                                 Random.Range(-extents.y, extents.y),
                                 Random.Range(-extents.z, extents.z));

            position = transform.TransformPoint(offset);                  

            instance = Instantiate(prefab, position, prefab.transform.rotation, transform);
            yield return null;
        }
    }
}
