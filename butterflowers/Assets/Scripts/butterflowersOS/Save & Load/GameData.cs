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

        [Header("Neue")]
	    public string username = null;
	    
	    public SurveillanceData[] surveillanceData = new SurveillanceData[]{};

	    public string[] directories = new string[]{};
	    public string[] files = new string[] { };
		
	    public ushort[] user_files = new ushort[] { };
	    public ushort[] shared_files = new ushort[] { };
	    public ushort[] world_files = new ushort[] { };

	    public Profile profile = new Profile();
	    public BrainData brain = new BrainData();

	    [Header("Miscellaneous")] 
	    public bool[] cutscenes = new bool[] { false, false };
    }

    public static class GameDataExtensions
    {
	    public static void ReadFromBrainData(this GameData data, BrainData brain)
	    {
		    data.username = brain.username;

		    data.surveillanceData = brain.surveillanceData;
		    
		    //data.directories = 
	    }
    }

}
