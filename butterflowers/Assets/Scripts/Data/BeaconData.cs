using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = Beacon.Type;

[System.Serializable]
public class BeaconData {
    public int type = 0;
    public string path = "";
    public bool visible = false;

    public BeaconData(string path, Type type, bool visible)
    {
        this.path = path;
        this.type = (int)type;
        this.visible = visible;
    }
}
