using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Objects.Base;
using UnityEngine;
using uwu.Camera.Instances;

[Obsolete("Obsolete API!", true)]
public class BeaconFocalPoint : Focusable
{
    // Start is called before the first frame update
    protected override void OnStart()
    {
        base.Start();
        overrideCamera = GameObject.Find("Beacon Camera").GetComponent<FocusCamera>();
    }
}
