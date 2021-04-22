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
        public string import_agent_created_at = "";

        [Header("Miscellaneous")] 
        
        public bool tutorial = false;
        public bool[] cutscenes = new bool[] { false, false, false };

        [Header("Neue")] 
        
        public string username = "";
        public string agent_created_at = "";
        public int agent_event_stack = 0;

        public SurveillanceData[] surveillanceData = new SurveillanceData[]{};

	    public string[] directories = new string[]{};
	    public string[] files = new string[] { };
		
	    public ushort[] user_files = new ushort[] { };
	    public ushort[] shared_files = new ushort[] { };
	    public ushort[] world_files = new ushort[] { };

	    public Profile profile = new Profile();
	    
	    [HideInInspector] public byte[] images = new byte[]{};
	    
	    public ushort image_height = 0;
	    public ushort image_width = 0;
    }

}
