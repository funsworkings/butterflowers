using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public float time = 0f;
    public int chapter = -1;
    public string[] discoveries = new string[] { };
    public BeaconData[] beacons = new BeaconData[] { };
    public bool nestopen = false;
}
