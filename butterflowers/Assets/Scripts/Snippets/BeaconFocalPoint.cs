using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconFocalPoint : FocalPoint
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        overrideCamera = GameObject.Find("Beacon Camera").GetComponent<FocusCamera>();
    }
}
