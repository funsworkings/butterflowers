using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    CameraManager CameraManager;

    public bool refresh = false;

    [SerializeField] bool continuous = true;

    // Start is called before the first frame update
    void Start()
    {
        CameraManager = FindObjectOfType<CameraManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (refresh || continuous) 
        {
            var camera = CameraManager.MainCamera;
            transform.forward = (camera.transform.position - transform.position).normalized;

            refresh = false;
        }
    }
}
