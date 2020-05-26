using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyGravityRelativeToCamera : ApplyCustomGravity
{
    Camera mainCamera;

    public float multiplier = 1f;
    public Vector3 gravity
    {
        get
        {
            return -mainCamera.transform.up;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    protected override void ApplyGravity(float magnitude)
    {
        directionOfGravity = gravity;
        base.ApplyGravity(magnitude * multiplier);
    }
}
