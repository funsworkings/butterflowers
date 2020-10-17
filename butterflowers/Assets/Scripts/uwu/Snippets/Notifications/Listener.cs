using System;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets.Notifications
{
	public class Listener : MonoBehaviour
	{
		[SerializeField] Controller controller;


		[SerializeField] NotificationEvent[] events = { };


		void OnEnable()
		{
			if (controller == null)
				return;

			controller.onDismissNotification += ReceiveNotificationEvent;
		}

		void OnDisable()
		{
			if (controller == null)
				return;

			controller.onDismissNotification -= ReceiveNotificationEvent;
		}

		void ReceiveNotificationEvent(Notification notification)
		{
			if (notification == null)
				return;


			foreach (var e in events)
				if (e.notification == notification)
					e.response.Invoke();
		}

		[Serializable]
		public struct NotificationEvent
		{
			public Notification notification;
			public UnityEvent response;
		}
	}
}