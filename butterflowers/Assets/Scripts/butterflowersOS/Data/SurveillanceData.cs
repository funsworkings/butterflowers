﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace butterflowersOS.Data
{
	/// <summary>
	/// Surveillance data captured over the course of a day
	/// </summary>
	[System.Serializable]
	public class SurveillanceData
	{
		/// <summary>
		/// Day of the surveillance data log
		/// </summary>
		public ushort timestamp = 0;
	
		[Header("File stats")]
			public ushort filesAdded = 0; // Gluttony, nurture
			public ushort filesRemoved = 0; // Destruction, spontaneity
			

		[Header("Log stats")] 
			public SurveillanceLogData[] logs = new SurveillanceLogData[]{};
			
		#region Log accessors

		public float averageHoB => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.butterflyHealth/255f).Average()); // Destruction, play, nurture
		public float averageNestFill => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.nestFill/255f).Average()); // Nurture, destruction, gluttony
		public float averageCursorSpeed => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.cursorSpeed).Average()); // Play, spontaneity

		// Rest, spontaneity, play
		public float timeSpentInNest => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.Nest.ToByte()).Count() * 1f) / logs.Length;
		public float timeSpentInMagicStar => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.MagicStar.ToByte()).Count() * 1f) / logs.Length;
		public float timeSpentInTree => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.Tree.ToByte()).Count() * 1f) / logs.Length;
		public float timeSpentInDefault => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.NULL.ToByte()).Count() * 1f) / logs.Length;
		
		#endregion

		// Accessors for core attributes driving AI
		public int discoveries => logs.Select(log => log.SortEventsByType(EVENTCODE.DISCOVERY).Count()).Sum(); // Spontaneity, play
		public int beaconsAdded => logs.Select(log => log.SortEventsByType(EVENTCODE.BEACONACTIVATE).Count()).Sum(); // Gluttony, destruction
		public int beaconsDestroyed => logs.Select(log => log.SortEventsByType(EVENTCODE.BEACONDELETE).Count()).Sum(); // Gluttony, destruction
		public int beaconsPlanted => logs.Select(log => log.SortEventsByType(EVENTCODE.BEACONPLANT).Count()).Sum(); // Nurture, rest
		public int beaconsFlowered => logs.Select(log => log.SortEventsByType(EVENTCODE.BEACONFLOWER).Count()).Sum(); // Nurture, rest
		public int nestKicks => logs.Select(log => log.SortEventsByType(EVENTCODE.NESTKICK).Count()).Sum(); // Play, rest
		public int nestSpills => logs.Select(log => log.SortEventsByType(EVENTCODE.NESTSPILL).Count()).Sum(); // Destruction, gluttony
	}

	public static class SurveillanceDataExtensions
	{
		public static bool AggregateSurveillanceData(this SurveillanceData current, SurveillanceData @new)
		{
			bool isValid = (current.timestamp == 0 || current.timestamp == @new.timestamp);
			if (isValid) 
			{
				current.timestamp = @new.timestamp;
				
				current.filesAdded = @new.filesAdded;
				current.filesRemoved = @new.filesRemoved;
				current.logs = @new.logs;
			}

			return isValid;
		}
	}
}