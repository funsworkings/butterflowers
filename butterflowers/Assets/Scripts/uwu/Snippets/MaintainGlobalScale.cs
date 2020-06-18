using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MaintainGlobalScale : MonoBehaviour
{
    [SerializeField] Vector3 scale;

    // Update is called once per frame
    void Update()
    {
        Vector3 globalscale = transform.lossyScale;

        float dx = scale.x / globalscale.x;
        float dy = scale.y / globalscale.y;
        float dz = scale.z / globalscale.z;

        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(dx, dy, dz));
    }
}
