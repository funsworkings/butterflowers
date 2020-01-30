using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : MonoBehaviour
{
    new Camera camera;

    [SerializeField] float distanceFromCamera = 10f;


    public Vector3 position {
        get{
            return Input.mousePosition;
        }
        set {
            transform.position = value;
        }
    }

    Vector3 m_velocity = Vector3.zero;
    public Vector3 velocity {
        get{
            return m_velocity;
        }
    }

    Vector3 a, b;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;

        a = b = position;
        m_velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        position = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera));

        b = position;
        m_velocity = (b - a) / dt;
        a = b;
    }
}
