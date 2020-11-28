using AI.Types;

namespace uwu
{
	[System.Serializable]
	public class BrainData : GameData
	{
		public string username = null;
		public bool load = false;
		
		public string[] files = new string[] { };
		
		public int[] discoveries = new int[] { };
		public int[] shared_files = new int[] { };
		
		public Knowledge[] file_knowledge = new Knowledge[] { };
		
		public BehaviourProfile behaviourProfile = new BehaviourProfile();
	}
}