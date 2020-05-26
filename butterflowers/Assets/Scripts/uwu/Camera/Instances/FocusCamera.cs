using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCamera : GameCamera
{
    [SerializeField] Transform focus = null;
    Transform defaultFocus = null;

    [SerializeField] bool follow = true, lookAt = true;

    protected override void Start()
    {
        base.Start();
        defaultFocus = focus;
    }

    public override void Disable()
    {
        base.Disable();
        LoseFocus();
    }

    public void Focus(Transform target)
    {
        focus = target;

        if (follow) camera.Follow = focus;
        if (lookAt) camera.LookAt = focus;
    }

    public void LoseFocus()
    {
        focus = defaultFocus;
    }
}
