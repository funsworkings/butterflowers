using AI.Types;
using Data;

namespace uwu
{
	[System.Serializable]
	public class SceneData : GameData
	{
		public bool active = false;
        
		public float time = 0f;
		public int chapter = -1;

		public BeaconData[] beacons = new BeaconData[] { };
		public VineData[] vines = new VineData[] { };

		public bool nestopen = false;

		public int dialoguenode = -1;
		public int[] dialoguevisited = new int[] { };
		public float enviro_knowledge = 0f;
		public float max_stance = -1f;

		public bool wizard = false;
		public LogData logs = new LogData();

		public int nestcapacity = 6;

		public World.State GAMESTATE = World.State.User;

		public bool[] corners = {false, false, false, false};

		public SurveillanceData[] surveillanceData = new SurveillanceData[]{};
	}
}