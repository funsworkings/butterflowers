using Neue.Agent.Brain.Data;
using uwu;

namespace butterflowersOS
{
	[System.Serializable]
	public class BrainData
	{
		public string username = null;

		public string[] directories = new string[]{};
		public string[] files = new string[] { };
		
		public int[] user_files = new int[] { };
		public int[] shared_files = new int[] { };
		public int[] world_files = new int[] { };

		public Knowledge[] file_knowledge = new Knowledge[] { };
		
		public Profile profile = new Profile();

		public BrainData(){}

		public BrainData(GameData dat)
		{
			this.username = dat.username;
			
			this.directories = dat.directories;
			this.files = dat.files;
			this.user_files = dat.user_files;
			this.shared_files = dat.shared_files;
			this.world_files = dat.world_files;

			this.file_knowledge = dat.file_knowledge;
			this.profile = dat.profile;
		}
	}
}