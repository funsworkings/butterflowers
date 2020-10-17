using System.Collections.Generic;
using UnityEngine;

namespace uwu.Events
{
	[CreateAssetMenu(fileName = "New Game Event", menuName = "Events/Default", order = 52)]
	public class GameEvent : ScriptableObject // 2
	{
		protected List<GameEventListener> listeners = new List<GameEventListener>();

		public virtual void Raise()
		{
			for (var i = listeners.Count - 1; i >= 0; i--)
				listeners[i].OnEventRaised(this);
		}

		public virtual void RegisterListener(GameEventListener listener)
		{
			listeners.Add(listener);
		}

		public virtual void UnregisterListener(GameEventListener listener)
		{
			listeners.Remove(listener);
		}
	}
}