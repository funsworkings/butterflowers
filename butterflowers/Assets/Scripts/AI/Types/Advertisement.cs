using System;
using AI.Types.Mappings;
using UnityEngine;

namespace AI.Types
{
	[System.Serializable]
	public class Advertisement
	{
		[SerializeField]
		string id = null;

		public string ID
		{
			get
			{
				if(id == null || id == "")
					id = System.Guid.NewGuid().ToString();

				return id;
			}
		}
		
		
		public Action action = new Action();
		public BehaviourIntGroup rewards = new BehaviourIntGroup();

		#region Actions

		public void SetActionParams(EVENTCODE @event, object data)
		{
			action.@event = @event;
			action.dat = data;
		}
		
		#endregion

		#region Rewarding

		public int GetReward(Behaviour behaviour, bool wipe)
		{
			var value = rewards.GetValue(behaviour);
			if (wipe) 
				rewards.SetValue(behaviour, 0);

			return value;
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (obj == null) return false;

			if (obj.GetType() == typeof(Advertisement)) {
				Advertisement other = (Advertisement) obj;
				return (ID == other.ID);
			}

			return false;
;		}
	}
}