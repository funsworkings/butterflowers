using Data;
using Objects.Types;

namespace uwu
{
	[System.Serializable]
	public class SceneData : GameData
	{
		public SunData sun = new SunData();
		
		public WorldState GAMESTATE = WorldState.User;

		public int chapter = 0;

		public BeaconData[] beacons = new BeaconData[] { };
		public VineSceneData vines = new VineSceneData(); 
		public SurveillanceData[] surveillanceData = new SurveillanceData[]{};
		
		public bool nestopen = false;
		
		public float enviro_knowledge = 0f;
	}
}