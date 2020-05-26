using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;
using UnityEngine.Events;

/// <summary>
/// Base class for game cameras in scene
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class GameCamera : MonoBehaviour
{
    #region Events

    public UnityEvent onEnable, onDisable;

	#endregion


	protected CameraManager manager;

    // Connect to Cinemachine camera
    protected new CinemachineVirtualCamera camera;
                protected CinemachineTransposer transposer;
                protected CinemachineComposer composer;

    protected CinemachineFreeLook cameraFreeLook;

    #region Monobehaviour callbacks

    protected virtual void Awake() {
        manager = FindObjectOfType<CameraManager>();

        camera = GetComponent<CinemachineVirtualCamera>();
        cameraFreeLook = GetComponent<CinemachineFreeLook>();
    }

    protected virtual void Start(){
        if (camera != null)
        {
            transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
            composer = camera.GetCinemachineComponent<CinemachineComposer>();
        }
    }

    public virtual void Enable(){ 
        Debug.Log("Enabled " + name);
        camera.enabled = true;

        onEnable.Invoke();
    }

    public virtual void Disable(){
        Debug.Log("Disabled " + name);
        camera.enabled = false;

        onDisable.Invoke();
    }

    #endregion
}
