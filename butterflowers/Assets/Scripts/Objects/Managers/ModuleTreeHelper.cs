using AI.Agent;
using AI.Types;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Noder.Nodes.Behaviours.Entries;

namespace Objects.Managers
{
	using Filter = FilterBeacons.Filter;
	
	public class ModuleTreeHelper : Entity
	{
		// Properties

		[SerializeField] Brain brain;
		[SerializeField] AI.Scriptables.BrainPreset brainPreset;
		[SerializeField] BrainPreset oldPreset;
		
		[SerializeField] BeaconManager beacons;
		[SerializeField] ButterflowerManager butterflowers;
		[SerializeField] Nest nest;
		
		// Accessors

		public BrainPreset OldPreset => oldPreset;
		public Nest Nest => nest;
		public ButterflowerManager Butterflowers => butterflowers;
		public Actions Actions => brain.Actions;

		public float stance => brain.stance;
		public float mood => brain.mood;

		public EVENTCODE LAST_PLAYER_EVENT => brain.LAST_PLAYER_EVENT;
		public EVENTCODE LAST_WORLD_EVENT => brain.LAST_WORLD_EVENT;
		public EVENTCODE LAST_NEST_EVENT => brain.LAST_NEST_EVENT;

		
		#region AI accessors
		
		public Mood getMoodState()
		{
			return brain.Mood;
		}

		public SecondaryMood getSubMoodState()
		{
			return brain.SecondaryMood;
		}

		public Stance getStanceState(AGENT agent = AGENT.NULL)
		{
			return brain.Stance;
		}

		public bool Self => brain.Self;
		public bool Remote => brain.isActive && !Self;
		
		#endregion
		
		#region Beacon filters
		
		public string[] getBeacons() { return beacons.LiveBeacons.Select(beacon => beacon.file).ToArray(); }

		public string[] filterBeacons(string[] raw, FilterBeacons.Filter filter)
		{
			var ek = brain.FetchKnowledgeFromEnvironment();

			List<Beacon> source = new List<Beacon>();
			foreach (string key in raw) {
				var beacon = beacons.GetBeaconByFile(key);
				if (beacon != null)
					source.Add(beacon);
			}

			switch (filter) 
			{
				case Filter.Active:
					source = source.Where(b => !b.visible).ToList();
					break;
				case Filter.Inactive:
					source = source.Where(b => b.visible).ToList();
					break;
				case Filter.Unknown:
					source = source.Where(b => isUnknown(b)).ToList();
					break;
				case Filter.Comfortable:
					source = source.Where(b => isComfortable(b)).ToList();
					break;
				case Filter.Memory:
					source = new List<Beacon>();
					break;
				case Filter.Playable:
				case Filter.Actionable:
					source = source.Where(b => isActionable(b)).ToList();
					break;
				default:
					break;
			}

			return source.Select(beacon => beacon.file).ToArray();
		}
		
		#endregion
		
		#region Beacon types
		
		public bool isActionable(Beacon beacon)
		{
			return true;
			return brain.FetchKnowledgeFromBeacon(beacon) > brain.FetchKnowledgeFromEnvironment();
		}

		public bool isComfortable(Beacon beacon)
		{
			return true;
			return brain.FetchKnowledgeFromBeacon(beacon) > oldPreset.actionableBeaconThreshold;
		}

		public bool isUnknown(Beacon beacon)
		{
			return false;
			return brain.FetchKnowledgeFromBeacon(beacon) <= oldPreset.unknownBeaconThreshold;
		}
		
		#endregion
		
		#region Camera

		public Focusable[] getWaypoints()
		{
			return World.Focusables;
		}

		#endregion
	}
}