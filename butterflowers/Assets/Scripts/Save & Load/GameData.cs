using System.Collections;
using System.Collections.Generic;
using Data;
using Neue.Agent.Brain.Data;
using Objects.Types;
using UnityEngine;

namespace uwu
{

    public partial class GameData
    {
	    [Header("Base")]
        public SunData sun = new SunData();

        public BeaconData[] beacons = new BeaconData[] { };
        public VineSceneData vines = new VineSceneData(); 
        public SurveillanceData[] surveillanceData = new SurveillanceData[]{};
		
        public bool nestopen = false;
		
        public float enviro_knowledge = 0f;
	        
	    [Header("Neue")]
	    public string username = null;

	    public string[] directories = new string[]{};
	    public string[] files = new string[] { };
		
	    public int[] user_files = new int[] { };
	    public int[] shared_files = new int[] { };
	    public int[] world_files = new int[] { };

	    public Knowledge[] file_knowledge = new Knowledge[] { };
		
	    public Profile profile = new Profile();
    }

}
