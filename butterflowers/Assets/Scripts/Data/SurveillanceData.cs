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
			public int filesAdded = 0;
			public int filesRemoved = 0;
			public int discoveries = 0;

		[Header("Log stats")] 
			public SurveillanceLogData[] logs;
			
		#region Log accessors

		public float averageHoB => (logs.Select(log => log.butterflyHealth).Average());
		public float averageNestFill => (logs.Select(log => log.nestFill).Average());
		public float averageCursorSpeed => (logs.Select(log => log.cursorSpeed).Average());

		public float timeSpentInNest => (logs.Where(log => log.agentInFocus == AGENT.Nest).Count() * 1f) / logs.Length;
		public float timeSpentInMagicStar => (logs.Where(log => log.agentInFocus == AGENT.MagicStar).Count() * 1f) / logs.Length;
		public float timeSpentInTree => (logs.Where(log => log.agentInFocus == AGENT.Tree).Count() * 1f) / logs.Length;
		public float timeSpentInDefault => (logs.Where(log => log.agentInFocus == AGENT.NULL).Count() * 1f) / logs.Length;
		
		#endregion

		[Header("Beacon stats")] 
			public int beaconsAdded = 0;
			public int beaconsPlanted = 0;

		[Header("Nest stats")] 
			public int nestKicks = 0;
			public int nestSpills = 0;
	}
}