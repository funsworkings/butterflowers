using System.Linq;
using UnityEngine;

namespace Data
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
		public int timestamp = 0;
	
		[Header("File stats")]
			public int filesAdded = 0; // Gluttony, nurture
			public int filesRemoved = 0; // Destruction, spontaneity
			public int discoveries = 0; // Spontaneity, play

		[Header("Log stats")] 
			public SurveillanceLogData[] logs = new SurveillanceLogData[]{};
			
		#region Log accessors

		public float averageHoB => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.butterflyHealth).Average()); // Destruction, play, nurture
		public float averageNestFill => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.nestFill).Average()); // Nurture, destruction, gluttony
		public float averageCursorSpeed => (logs == null || logs.Length == 0)? 0f:(logs.Select(log => log.cursorSpeed).Average()); // Play, spontaneity

		// Rest, spontaneity, play
		public float timeSpentInNest => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.Nest).Count() * 1f) / logs.Length;
		public float timeSpentInMagicStar => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.MagicStar).Count() * 1f) / logs.Length;
		public float timeSpentInTree => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.Tree).Count() * 1f) / logs.Length;
		public float timeSpentInDefault => (logs == null || logs.Length == 0)? 0f:(logs.Where(log => log.agentInFocus == AGENT.NULL).Count() * 1f) / logs.Length;
		
		#endregion

		[Header("Beacon stats")] 
			public int beaconsAdded = 0; // Gluttony, destruction
			public int beaconsPlanted = 0; // Nurture, rest

		[Header("Nest stats")] 
			public int nestKicks = 0; // Play, rest
			public int nestSpills = 0; // Destruction, gluttony
	}
}