using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Data;
using Objects.Entities;

[System.Serializable]
public class VineData
{
    public int status;

    public int index = 0;
    public float interval = 0f;
    public float height = 0f;

    public string file = "";

    public float[] waypoints_x;
    public float[] waypoints_y;
    public float[] waypoints_z;

    public LeafBundleData leaves;

    public VineData(Vine.Status status, int index, float interval, float height, Vector3[] waypoints, string file, Leaf[] leaves)
    {
        this.status = (int)status;
        this.index = index;
        this.interval = interval;
        this.height = height;

        waypoints_x = waypoints.Select(w => w.x).ToArray();
        waypoints_y = waypoints.Select(w => w.y).ToArray();
        waypoints_z = waypoints.Select(w => w.z).ToArray();

        this.file = file;

        this.leaves = new LeafBundleData();
        this.leaves.leaves = (leaves != null)? leaves.Select(l => new LeafData(l)).ToArray():null;
    }
}
