using Neue.Agent.Brain.Data;
using Neue.Types;

namespace uwu
{
	[System.Serializable]
	public class BrainData : GameData
	{
		public string username = null;
		public bool load = false;
		
		public string[] directories = new string[]{};
		public string[] files = new string[] { };
		
		public int[] user_files = new int[] { };
		public int[] shared_files = new int[] { };
		public int[] world_files = new int[] { };

		public Knowledge[] file_knowledge = new Knowledge[] { };
		
		public Profile profile = new Profile();
	}
}