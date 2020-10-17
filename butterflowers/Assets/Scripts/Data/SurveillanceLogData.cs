using UnityEngine;

namespace Data
{
	/// <summary>
	/// Log data that is captured at a set interval
	/// </summary>
	[System.Serializable]
	public class SurveillanceLogData
	{
		/// <summary>
		/// Time of day in relative units
		/// </summary>
		public float timestamp = 0f;
		
		public AGENT agentInFocus = AGENT.NULL;
		public float nestFill = 0f;
		public float butterflyHealth = 0f;
		public float cursorSpeed = 0f;
	}
}