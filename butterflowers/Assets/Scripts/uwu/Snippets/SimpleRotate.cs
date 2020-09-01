using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] Vector3 axis = Vector3.up;
    [SerializeField] float speed = 1f;

    [SerializeField] bool global = false;
    [SerializeField] bool randomSpeed = false;
    [SerializeField] bool randomAxis = false;


    public float Speed {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
        }
    }

    private void Start()
    {
        if (randomSpeed) speed *= Random.Range(0f, 1f);

        if (randomAxis) axis *= Random.Range(0f, 1f);
    }
    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        transform.Rotate(axis, speed * dt, (global)? Space.World:Space.Self);
    }

    void ReceiveEvents(Vector3 position, Vector3 normal){
        speed = position.y * (45f);
    }
}