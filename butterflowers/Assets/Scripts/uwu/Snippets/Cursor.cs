using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] Vector3 m_position;
    public Vector3 position {
        get{
            return m_position;
        }
    }

    [SerializeField] Vector3 m_velocity = Vector3.zero;
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

    protected virtual void Update() {
        m_position = Position();

        float dt = Time.deltaTime;

        b = position;
        m_velocity = (b - a) / dt;
        a = b;

        speed = velocity.magnitude;    
    }

    protected virtual Vector3 Position()
    {
        return Input.mousePosition;
    }
}
