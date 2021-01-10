using Neue.Agent.Brain.Data;

namespace butterflowersOS.Data
{
	public class BrainPayload
	{
		public BrainData behaviourProfile = new BrainData();
		
		public float environmentKnowledge;
		public Knowledge[] fileKnowledge = new Knowledge[]{};

		public BrainPayload(BrainData profile, float enviro, Knowledge[] files)
		{
			this.behaviourProfile = profile;
			this.environmentKnowledge = enviro;
			this.fileKnowledge = files;
		}
	}
}