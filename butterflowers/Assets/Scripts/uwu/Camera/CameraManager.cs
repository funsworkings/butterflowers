using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

/// <summary>
/// Manages all in-game cameras
///     - Ensure one camera used at a time
///     - Switch between camera views
/// </summary>

public class CameraManager: MonoBehaviour {
    [SerializeField] GameCamera defaultCamera, previousCamera, currentCamera;

    [SerializeField] Camera m_mainCamera = null;


    int virtualCameraIndex = -1;
    List<GameCamera> virtualCameras = new List<GameCamera>();

    public Camera MainCamera {
        get
        {
            if (m_mainCamera == null)
                m_mainCamera = Camera.main;

            return m_mainCamera;
        }
        set
        {
            m_mainCamera = value;
        }
    }

    public GameCamera DefaultCamera => defaultCamera;

    public GameCamera ActiveCamera {
        get
        {
            return currentCamera;
        }
    }


    #region Monobehaviour callbacks

    // Start is called before the first frame update
    void Start()
    {
        virtualCameras = GetComponentsInChildren<GameCamera>(true).ToList();
        foreach (GameCamera c in virtualCameras) {
            if (c == defaultCamera) Enable(c);
            else Disable(c);
        }
    }

    #endregion

    void Enable(GameCamera camera){
        if(camera == null) return;
             
        if(currentCamera != camera){
            previousCamera = currentCamera;
            Disable(previousCamera);

            camera.Enable();
            currentCamera = camera;
            onUpdateGameCamera();
        }
    }

    void Disable(GameCamera camera){
        if(camera == null) return;

        camera.Disable();
        if(currentCamera == camera){
            previousCamera = currentCamera;

            currentCamera = null;
            onUpdateGameCamera();
        }
    }

    #region Operations

    // Reset to default camera
    public void ResetToDefault(){
        if(currentCamera != defaultCamera){
            Disable(currentCamera);
            Enable(defaultCamera);
        }
    }

    public void SetCamera(GameCamera camera)
    {
        Enable(camera);
    }

    public void ClearCamera(GameCamera camera)
    {
        Disable(camera);
    }

    public void CycleCameras(){
        int index = virtualCameraIndex + 1;
        if(index > virtualCameras.Count-1)
            index = 0;

        Enable(virtualCameras[index]);
    }

    #endregion

    #region Callbacks

    void onUpdateGameCamera(){
        if(currentCamera == null){
            virtualCameraIndex = -1;
            return;
        }

        virtualCameraIndex = virtualCameras.IndexOf(currentCamera);
    }

    #endregion

    #region Deprecated

    /*

    Camera mainCamera;
    [SerializeField] new Camera camera;
     int cameraIndex = -1;
     List<Camera> cameras = new List<Camera>();

     void Start(){

         cameras = FindObjectsOfType<Camera>().ToList();
        foreach(Camera c in cameras){
            if(c != mainCamera && c.tag != "Hidden")
                c.gameObject.SetActive(false);
            else 
                camera = c;
        }
     }

     void onUpdateCamera(){
        if(camera == null){
            cameraIndex = -1;
            return;
        }

        cameraIndex = cameras.IndexOf(camera);
    }

    public void Set(Camera camera){
        if(this.camera != camera){
            if(this.camera != null && this.camera.tag != "Hidden")
                this.camera.gameObject.SetActive(false);
            this.camera = camera;

            if (this.camera != null && this.camera.tag != "Hidden")
                this.camera.gameObject.SetActive(true);
        }
    }

    public void Set(GameCamera camera){
        if(camera == null)
            return;

        if(currentCamera != null && currentCamera != camera)
            Disable(currentCamera); // Disable previous camera

        Enable(camera);    
    }


    */

    #endregion
}
