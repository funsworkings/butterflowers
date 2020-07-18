using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

public partial class GameData {
    public string[] files = new string[] { };

    // File lookups
    public int[] discoveries = new int[] { };
    public int[] shared_files = new int[] { };

    public float time = 0f;
    public int chapter = -1;

    public BeaconData[] beacons = new BeaconData[] { };
    public bool nestopen = false;

    public int dialoguenode = -1;
    public int[] dialoguevisited = new int[] { };
    public float enviro_knowledge = 0f;
    public Knowledge[] file_knowledge = new Knowledge[] { };

    public bool wizard = false;
    public LogData logs = new LogData();

    public int nestcapacity = 6;

    public int GAMESTATE = 0;
}
