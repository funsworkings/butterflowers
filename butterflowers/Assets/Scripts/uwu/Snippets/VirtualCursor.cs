using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCursor : Cursor
{
    [SerializeField] new Camera camera;

    protected override Vector3 Position()
    {
        return camera.WorldToScreenPoint(transform.position);
    }
}
