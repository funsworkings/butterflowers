using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineDriver : MonoBehaviour
{
    new CinemachineVirtualCamera camera;

    CinemachineTransposer transposer;
    CinemachineComposer composer;

    void Awake() {
        Fetch();
    }

    bool fetched = false;
    void Fetch(){
        camera = GetComponent<CinemachineVirtualCamera>();
        transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
        composer = camera.GetCinemachineComponent<CinemachineComposer>(); 

        fetched = true;
    }

    public void SetNearClipping(float near){
        if(!fetched) Fetch();
        (camera.m_Lens).NearClipPlane = near;
    }

    public void SetFarClipping(float far){
        if(!fetched) Fetch();
        (camera.m_Lens).FarClipPlane = far;
    }

    public void UpdateTransposerOffset(Vector3 offset){
        if(!fetched) Fetch();
        transposer.m_FollowOffset = offset;
    }

    public void UpdateComposerOffset(Vector3 offset){
        if(!fetched) Fetch();
        composer.m_TrackedObjectOffset = offset;
    }
}
