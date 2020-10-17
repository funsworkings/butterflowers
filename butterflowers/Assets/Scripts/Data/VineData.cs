using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

[System.Serializable]
public class VineData
{
    public int status;

    public int index = 0;
    public float interval = 0f;

    public string file = "";

    public float[] waypoints_x;
    public float[] waypoints_y;
    public float[] waypoints_z;

    public VineData(Vine.Status status, int index, float interval, Vector3[] waypoints, string file)
    {
        this.status = (int)status;
        this.index = index;
        this.interval = interval;

        waypoints_x = waypoints.Select(w => w.x).ToArray();
        waypoints_y = waypoints.Select(w => w.y).ToArray();
        waypoints_z = waypoints.Select(w => w.z).ToArray();

        this.file = file;
    }
}
