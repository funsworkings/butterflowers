using Neue.Reference.Types;
using Neue.Reference.Types.Maps.Groups;
using Neue.Types;

namespace Neue.Agent.Brain.Data
{
	[System.Serializable]
	public class Profile
	{
		public FrameFloatGroup weights;

		#region Access

		public float GetWeight(Frame frame)
		{
			return weights.GetValue(frame);
		}
		
		public float GetDecay(Frame frame)
		{
			if (frame == Frame.Quiet) 
				return -(1f - GetWeight(frame) + .1f);

			return GetWeight(frame);
		}
		
		#endregion
	}
}