using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Type = Beacon.Type;
using Locale = Beacon.Locale;

[System.Serializable]
public class BeaconData 
{
    public int type = 0;
    public int state = -1;

    public string path = "";

    public float x;
    public float y;
    public float z;

    public BeaconData(string path, Vector3 origin, Type type, Locale state)
    {
        this.path = path;
        this.type = (int)type;
        this.state = (int)state;

        this.x = origin.x;
        this.y = origin.y;
        this.z = origin.z;
    }
}
