using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using AI.Types;

namespace uwu
{

    public partial class GameData
    {
        public string username = null;
        
        public string[] files = new string[] { };

        // File lookups
        public int[] discoveries = new int[] { };
        public int[] shared_files = new int[] { };

        public bool active = false;
        
        public float time = 0f;
        public int chapter = -1;

        public BeaconData[] beacons = new BeaconData[] { };
        public VineData[] vines = new VineData[] { };

        public bool nestopen = false;

        public int dialoguenode = -1;
        public int[] dialoguevisited = new int[] { };
        public float enviro_knowledge = 0f;
        public Knowledge[] file_knowledge = new Knowledge[] { };
        public float max_stance = -1f;

        public bool wizard = false;
        public LogData logs = new LogData();

        public int nestcapacity = 6;

        public World.State GAMESTATE = World.State.User;

        public bool[] corners = {false, false, false, false};

        public SurveillanceData[] surveillanceData = new SurveillanceData[]{};
    }

}
