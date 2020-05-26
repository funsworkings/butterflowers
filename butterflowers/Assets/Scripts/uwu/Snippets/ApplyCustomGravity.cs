using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ApplyCustomGravity : MonoBehaviour
{
    protected new Rigidbody rigidbody;
    protected Vector3 directionOfGravity = Vector3.down;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rigidbody.useGravity = false;

        var magnitude = Physics.gravity.magnitude;
        ApplyGravity(magnitude);
    }

    protected virtual void ApplyGravity(float magnitude)
    {
        rigidbody.AddForce(directionOfGravity.normalized * magnitude);
    }
}
