using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Events
{ // 1

	[Serializable]
	public class CustomGameEvent
	{
		public GameEvent gameEvent;
		public UnityEvent response;
	}

	public class GameEventListener : MonoBehaviour
	{
		[SerializeField] List<CustomGameEvent> events = new List<CustomGameEvent>();

		[SerializeField] List<CustomGameEvent> objectEvents = new List<CustomGameEvent>();

		[SerializeField] GameObject eventObject;
		readonly Dictionary<GameEvent, CustomGameEvent> eventsDict = new Dictionary<GameEvent, CustomGameEvent>();
		public GameObject EventObject => eventObject;

		void Start()
		{
			foreach (var e in events)
				eventsDict.Add(e.gameEvent, e);

			foreach (var e in objectEvents)
				eventsDict.Add(e.gameEvent, e);
		}

		void OnEnable() // 4
		{
			foreach (var e in events)
				e.gameEvent.RegisterListener(this);

			foreach (var e in objectEvents)
				e.gameEvent.RegisterListener(this);
		}

		void OnDisable() // 5
		{
			foreach (var e in events)
				e.gameEvent.UnregisterListener(this);

			foreach (var e in objectEvents)
				e.gameEvent.UnregisterListener(this);
		}

		public void OnEventRaised(GameEvent e) // 6
		{
			CustomGameEvent cge = null;

			eventsDict.TryGetValue(e, out cge);
			if (cge != null)
				cge.response.Invoke();
		}

		public void OnEventRaised(GameEvent e, GameObject o)
		{ //Used for object-oriented events
			CustomGameEvent cge = null;

			eventsDict.TryGetValue(e, out cge);
			if (cge != null) {
				if (!objectEvents.Contains(cge))
					return;

				eventObject = o;
				cge.response.Invoke();
			}
		}
	}
}