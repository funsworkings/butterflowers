using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    #region Events

    public System.Action<Vector3, Vector3> onGrab, onContinue, onRelease;

    #endregion

    #region External interaction

    public void Grab(RaycastHit hit)
    {
        if (onGrab != null)
            onGrab(hit.point, hit.normal);
    }

    public void Continue(RaycastHit hit)
    {
        if (onContinue != null)
            onContinue(hit.point, hit.normal);
    }

    public void Release(RaycastHit hit)
    {
        if (onRelease != null)
            onRelease(hit.point, hit.normal);
    }

    #endregion
}
