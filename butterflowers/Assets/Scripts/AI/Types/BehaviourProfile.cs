using System.Collections.Generic;
using AI.Types.Mappings;
using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class BehaviourProfile
	{
		public BehaviourFloatGroup weights;

		#region Access

		public float GetWeight(Behaviour behaviour)
		{
			return weights.GetValue(behaviour);
		}
		
		public float GetDecay(Behaviour behaviour)
		{
			if (behaviour == Behaviour.REST) 
				return -(1f - GetWeight(behaviour) + .1f);

			return GetWeight(behaviour);
		}
		
		#endregion
	}
}