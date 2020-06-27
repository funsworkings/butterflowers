using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

[System.Serializable]
public class GameData
{
    public string BUILD_VERSION = "0.0";
    public string TIMESTAMP = "0";

    public float time = 0f;
    public int chapter = -1;
    public string[] discoveries = new string[] { };
    public BeaconData[] beacons = new BeaconData[] { };
    public bool nestopen = false;

    public int dialoguenode = -1;
    public int[] dialoguevisited = new int[] { };
    public float enviro_knowledge = 0f;
    public Knowledge[] file_knowledge = new Knowledge[] { };
    public string[] shared_files = new string[] { };

    public bool wizard = false;
    public Scribe.Log[] logs = new Scribe.Log[] { };

    public int nestcapacity = 6;
}
