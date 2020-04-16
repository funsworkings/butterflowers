using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public Vector3 position {
        get{
            return Input.mousePosition;
        }
    }

    Vector3 m_velocity = Vector3.zero;
    public Vector3 velocity {
        get{
            return m_velocity;
        }
    }

    public float speed;

    Vector3 a, b;


    void Start() {
        a = b = position;
        m_velocity = Vector3.zero;    
    }

    void Update() {
        float dt = Time.deltaTime;

        b = position;
        m_velocity = (b - a) / dt;
        a = b;

        speed = velocity.magnitude;    
    }
}
