using UnityEngine;

namespace butterflowersOS.AI.Objects
{
	[System.Serializable]
	public class Point
	{
		public EVENTCODE @event;
		public Vector3 position;

		public Point(EVENTCODE @event, Vector3 point)
		{
			this.@event = @event;
			this.position = point;
		}
	}
}