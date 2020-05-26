using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] new Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        if(camera == null)
            camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(camera == null)
            return;
            
        transform.forward = (camera.transform.position - transform.position).normalized;
    }
}
