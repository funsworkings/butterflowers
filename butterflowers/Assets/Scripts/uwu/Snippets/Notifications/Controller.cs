using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uwu.Snippets.Notifications
{
	using instance = Instance;

	public class Controller : MonoBehaviour
	{
		[SerializeField] float clearTime = 1f;

		[SerializeField] Transform staticContainer = null;
		[SerializeField] GameObject staticPrefab = null;

		[SerializeField] Transform nonstaticContainer = null;
		[SerializeField] GameObject nonstaticPrefab = null;
		bool clearing;

		public Dictionary<Notification, instance> instances = new Dictionary<Notification, instance>();
		readonly List<Notification> nonstaticNotifications = new List<Notification>();
		public Action<Notification> onDismissNotification;

		public Action<Notification> onReceiveNotification;
		GameDataSaveSystem Save;

		readonly List<Notification> staticNotifications = new List<Notification>();

		public void Receive(Notification notification)
		{
			instance instance = null;
			instances.TryGetValue(notification, out instance);

			if (instance == null) {
				if (notification.isStatic) {
					staticNotifications.Add(notification);

					if (!clearing) {
						StartCoroutine("Clear");
						clearing = true;
					}
				}
				else {
					nonstaticNotifications.Add(notification);
				}

				AddToContainer(notification, notification.isStatic);

				if (onReceiveNotification != null)
					onReceiveNotification(notification);
			}
		}

		void onDismissedNotification(Notification notification)
		{
			Dismiss(notification, true);
		}

		public void Dismiss(Notification notification)
		{
			Dismiss(notification, false);
		}

		public void Dismiss(Notification notification, bool events = false)
		{
			instance instance = null;
			instances.TryGetValue(notification, out instance);

			if (instance != null) {
				if (notification.isStatic) {
					staticNotifications.Remove(notification);
				}
				else {
					nonstaticNotifications.Remove(notification);
					instance.onDismissed -= onDismissedNotification;
				}

				Destroy(instance.gameObject);
				instances.Remove(notification);

				if (events)
					if (onDismissNotification != null)
						onDismissNotification(notification);
			}
		}

		public void ClearAll()
		{
			var active = new List<Notification>();
			foreach (var notification in instances) {
				var notif = notification.Key;
				if (notif != null)
					active.Add(notif);
			}

			var activeArr = active.ToArray();
			foreach (var notif in activeArr)
				Dismiss(notif);
		}

		public void AddToContainer(Notification notification, bool isStatic)
		{
			var container = isStatic ? staticContainer : nonstaticContainer;
			var prefab = isStatic ? staticPrefab : nonstaticPrefab;

			var obj = Instantiate(prefab, container);

			var obj_i = obj.GetComponent<instance>();
			obj_i.notification = notification;

			if (!isStatic)
				obj_i.onDismissed += onDismissedNotification;

			instances.Add(notification, obj_i);
		}

		IEnumerator Clear()
		{
			var t = 0f;

			while (staticNotifications.Count > 0) {
				if (t >= clearTime) {
					var count = staticNotifications.Count;
					Dismiss(staticNotifications[count - 1]);

					t = 0f;
				}
				else {
					t += Time.deltaTime;
				}

				yield return null;
			}

			clearing = false;
		}
	}
}