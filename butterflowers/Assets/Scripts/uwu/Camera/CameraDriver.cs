using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDriver : Singleton<CameraDriver>
{
    Camera m_camera;
    public new Camera camera {
        get
        {
            if(m_camera == null)
                m_camera = Camera.main;

            return m_camera;
        }
    }

    public Vector3 MoveRelativeToCamera(Vector2 vector){
        return MoveRelativeToCamera(new Vector3(vector.x, vector.y, 0f));
    }

    public Vector3 MoveRelativeToCamera(Vector3 vector){
        vector.z = 0f;
        return camera.transform.TransformVector(vector);
    }

    public Vector3 ConvertToScreen(Vector3 position){
        return camera.WorldToScreenPoint(position);
    }

    public Vector3 ConvertToViewport(Vector3 position){
        return camera.WorldToViewportPoint(position);
    }
}
