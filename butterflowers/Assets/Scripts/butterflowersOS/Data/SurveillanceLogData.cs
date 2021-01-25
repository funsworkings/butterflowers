using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace butterflowersOS.Data
{
	/// <summary>
	/// Log data that is captured at a set interval
	/// </summary>
	[System.Serializable]
	public class SurveillanceLogData
	{
		public sbyte agentInFocus = -1;
		
		public byte nestFill = 0;
		public byte butterflyHealth = 0;

		public sbyte cursorX = 0;
		public sbyte cursorY = 0;
		
		public Vector2 cursorVelociy => new Vector2(cursorX, cursorY) * Constants.BaseCursorVelocityVector;
		public float cursorSpeed => cursorVelociy.magnitude;

		public List<sbyte> events = new List<sbyte>();

		public void AppendEvents(EVENTCODE[] events)
		{
			this.events.AddRange(events.Select(e => e.ToByte()));
		}
		
		public IEnumerable<EVENTCODE> SortEventsByType(EVENTCODE @eventcode)
		{
			var _events = events.Where(e => e == @eventcode.ToByte());
			return _events.Select(_e => EventCodeExtensions.FromByte(_e));
		}
	}
}