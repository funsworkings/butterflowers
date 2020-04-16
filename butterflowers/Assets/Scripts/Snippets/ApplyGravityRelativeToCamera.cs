using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyGravityRelativeToCamera : ApplyCustomGravity
{
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    protected override void ApplyGravity(float magnitude)
    {
        directionOfGravity = -mainCamera.transform.up;
        base.ApplyGravity(magnitude);
    }
}
