using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : MonoBehaviour
{
    new Camera camera;
    
    [SerializeField] Cursor cursor;
    [SerializeField] float distanceFromCamera = 10f;


    public Vector3 position {
        get{
            return cursor.position;
        }
        set {
            transform.position = value;
        }
    }

    public Vector3 velocity {
        get{
            return cursor.velocity;
        }
    }

    public float speed {
        get{
            return cursor.speed;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }
}
