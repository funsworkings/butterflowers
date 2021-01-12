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
        public SurveillanceData[] surveillanceData = new SurveillanceData[]{};
        public SequenceData sequence = new SequenceData();
		
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
	    public BrainData brain = new BrainData();
	    
	    public bool profileGenerated = false;
    }

}
