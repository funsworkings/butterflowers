using UnityEngine;

namespace uwu.Events
{
	[CreateAssetMenu(fileName = "New Object Event", menuName = "Events/Object", order = 52)] // 1
	public class ObjectEvent : GameEvent
	{
		public void Raise(GameObject obj)
		{
			for (var i = listeners.Count - 1; i >= 0; i--)
				listeners[i].OnEventRaised(this, obj);
		}
	}
}