using AI.Types;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using AI.Types.Mappings;
using Behaviour = AI.Types.Behaviour;

namespace AI
{
	public class Advertiser : MonoBehaviour
	{
		// Events

		public static System.Action<Advertisement, Behaviour, int> onCollectReward;
		
		// Properties

		[SerializeField] Advertisement[] m_Advertisements;
		Dictionary<EVENTCODE, Advertisement> m_advertisementLookup;
		
		#region Accessors

		public Advertisement[] Advertisements {
			get
			{
				m_Advertisements = advertisementLookup.Values.ToArray();
				return m_Advertisements;
			}
		}

		public Dictionary<EVENTCODE, Advertisement> advertisementLookup
		{
			get
			{
				if (m_advertisementLookup == null) 
					m_advertisementLookup = new Dictionary<EVENTCODE, Advertisement>();

				return m_advertisementLookup;
			}
		}

		#endregion
		
		#region Rewards

		public void CaptureAllRewards(EVENTCODE @event, bool wipe)
		{
			if (advertisementLookup.ContainsKey(@event)) 
			{
				var advertisement = advertisementLookup[@event];
				var behaviours = advertisement.rewards.behaviours;

				foreach (Behaviour b in behaviours)
					CaptureReward(@event, b, wipe);
			}
		}

		public int CaptureReward(EVENTCODE @event, Behaviour behaviour, bool wipe)
		{
			int reward = Advertisement.GetDefaultReward();
			
			if (advertisementLookup.ContainsKey(@event)) 
			{
				var advertisement = advertisementLookup[@event];
				reward = advertisement.GetReward(behaviour, wipe);

				if (onCollectReward != null)
					onCollectReward(advertisement, behaviour, reward);
			}

			return reward;
		}

		public void UpdateReward(EVENTCODE @event, object data, Behaviour behaviour, int reward)
		{
			Advertisement advertisement = null;

			if (!advertisementLookup.TryGetValue(@event, out advertisement)) {
				advertisement = new Advertisement();
			}
			
			advertisement.SetActionParams(@event, data, this);
			advertisement.SetReward(behaviour, reward);

			advertisementLookup[@event] = advertisement;
		}

		#endregion
	}
}