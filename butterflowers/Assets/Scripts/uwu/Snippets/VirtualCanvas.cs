using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VirtualCanvas : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] float thickness = 1f;

    // Update is called once per frame
    void Update()
    {
        if (camera == null) return;

        float depth = (camera.nearClipPlane + (0.01f + thickness/2f));

        transform.parent = camera.transform;

        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = new Vector3(0f, 0f, depth);

        float height = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * depth * 2f;
        transform.localScale = new Vector3(height * camera.aspect, height, thickness);
    }
}
