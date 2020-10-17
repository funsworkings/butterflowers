using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uwu.Camera.Instances;

public class BeaconFocalPoint : Focusable
{
    // Start is called before the first frame update
    protected override void OnStart()
    {
        base.Start();
        overrideCamera = GameObject.Find("Beacon Camera").GetComponent<FocusCamera>();
    }
}
