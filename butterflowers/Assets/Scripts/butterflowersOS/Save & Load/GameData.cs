using butterflowersOS;
using butterflowersOS.Data;
using Neue.Agent.Brain.Data;
using UnityEngine;

namespace uwu
{

    public partial class GameData
    {
	    [Header("Base")]
        public SunData sun = new SunData();

        public BeaconSceneData beacons = new BeaconSceneData();
        public VineSceneData vines = new VineSceneData();
        public SequenceData sequence = new SequenceData();

        public string export_agent_created_at = "";
        public bool export = false;
        
        [Header("Miscellaneous")] 
        public bool[] cutscenes = new bool[] { false, false };

        [Header("Neue")] 
        
        public string username = "";
        public string agent_created_at = "";

        public SurveillanceData[] surveillanceData = new SurveillanceData[]{};

	    public string[] directories = new string[]{};
	    public string[] files = new string[] { };
		
	    public ushort[] user_files = new ushort[] { };
	    public ushort[] shared_files = new ushort[] { };
	    public ushort[] world_files = new ushort[] { };

	    public Profile profile = new Profile();
    }

}
