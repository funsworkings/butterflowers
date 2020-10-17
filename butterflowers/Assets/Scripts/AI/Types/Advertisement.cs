using System;
using AI.Types.Mappings;
using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class Advertisement
	{
		public Action action = new Action();
		public BehaviourInt rewards = new BehaviourInt();
		
		
		#region Actions

		public void SetActionParams(EVENTCODE @event, object data, Advertiser advertiser)
		{
			action.@event = @event;
			action.dat = data;
			action.advertiser = advertiser;
		}
		
		#endregion

		#region Rewarding

		public int GetReward(Behaviour behaviour, bool wipe)
		{
			try {
				var reward = rewards.GetValue(behaviour);

				if (wipe)
					rewards.SetValue(behaviour, 0);

				return reward;
			}
			catch (System.Exception err) 
			{
				return GetDefaultReward();
			}
			
		}

		public void SetReward(Behaviour behaviour, int reward)
		{
			rewards.SetValue(behaviour, reward);
		}

		public static int GetDefaultReward()
		{
			return 0;
		}
		
		#endregion
	}
}