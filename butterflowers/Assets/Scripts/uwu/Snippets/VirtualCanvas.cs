using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VirtualCanvas : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] float thickness = 1f;

    float pr_fov = -1f;
    float pr_ncp = -1f;
    float pr_asp = -1f;

    // Update is called once per frame
    void Update()
    {
        if (camera == null) return;

        float ncp = camera.nearClipPlane;
        float fov = camera.fieldOfView;
        float asp = camera.aspect;

        if (ncp != pr_ncp || fov != pr_fov || asp != pr_asp) 
        {
            float depth = (ncp + (0.01f + thickness / 2f));

            transform.parent = camera.transform;

            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = new Vector3(0f, 0f, depth);

            float height = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) * depth * 2f;
            transform.localScale = new Vector3(height * asp, height, thickness);

            pr_asp = asp;
            pr_fov = fov;
            pr_ncp = ncp;
        }
    }
}
