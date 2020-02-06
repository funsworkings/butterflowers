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
        if(butterflyPrefab != null)
            StartCoroutine("Spawn"); // Begin
    }

    IEnumerator Spawn(){
        GameObject instance = null;
        Vector3 offset = Vector3.zero, position = Vector3.zero;

        while(true){
            if(transform.childCount < amount){
                if(camera == null){
                    offset = new Vector3(Random.Range(-extents.x, extents.x),
                                        Random.Range(-extents.y, extents.y),
                                        Random.Range(-extents.z, extents.z));

                    position = transform.TransformPoint(offset);   
                }
                else {
                    offset = new Vector3(Random.Range(0f, 1f),
                                        Random.Range(0f, 1f),
                                        Random.Range(extents.z, 2f * extents.z));
                    
                    position = camera.ViewportToWorldPoint(offset);
                }               

                instance = Instantiate(butterflyPrefab, position, butterflyPrefab.transform.rotation, transform);
            }

            yield return null;
        }
    }
}
