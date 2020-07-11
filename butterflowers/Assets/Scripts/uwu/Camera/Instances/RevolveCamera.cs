using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

public class RevolveCamera : GameCamera
{
    [SerializeField] Transform target = null;
    [SerializeField] bool controls = false;

    CinemachineOrbitalTransposer orbitTransposer = null;

    float defaultAngle = 0f;

    [Header("Attributes")]
        [SerializeField] float radius = 1f, height = 0f;
        [SerializeField] float angle = 0f;
        [SerializeField] bool resetOnDisable = false;

    protected override void Start()
    {
        base.Start();

        orbitTransposer = camera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        defaultAngle = angle;
    }

    // Update is called once per frame
    void Update()
    {
        if (orbitTransposer == null) return;

        /*
        float dt = Time.deltaTime;
        
        if (controls) {
            if (Input.GetKey(KeyCode.LeftArrow)) angle += 90f * dt;
            if (Input.GetKey(KeyCode.RightArrow)) angle -= 90f * dt;
        }
        */

        float angToRad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angToRad)*radius, height, Mathf.Sin(angToRad)*radius);

        orbitTransposer.m_FollowOffset = offset;
    }

    protected override void onDisabled()
    {
        base.onDisabled();

        if (resetOnDisable) 
        {
            //angle = defaultAngle;
        }
    }
}
