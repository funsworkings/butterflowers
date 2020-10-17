using System.Collections.Generic;
using AI.Types.Mappings;
using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class BehaviourProfile
	{
		public BehaviourFloat weight;
		public BehaviourFloat decay;

		public float baseDecaySpeed = 1f;
		
		#region Access

		public float GetWeight(Behaviour behaviour)
		{
			return weight.GetValue(behaviour);
		}
		
		public float GetDecay(Behaviour behaviour)
		{
			return decay.GetValue(behaviour);
		}
		
		#endregion
		
		#region Disposal

		public void Dispose()
		{
			weight.Dispose();
			decay.Dispose();
		}
		
		#endregion
	}
}