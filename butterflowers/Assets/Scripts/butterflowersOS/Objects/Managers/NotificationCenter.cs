using System;
using butterflowersOS.Core;
using butterflowersOS.Presets;
using butterflowersOS.Snippets;
using butterflowersOS.UI.Notifications;
using UnityEngine;
using uwu.UI.Behaviors.Visibility;
using Random = UnityEngine.Random;

namespace butterflowersOS.Objects.Managers
{
	public class NotificationCenter : MonoBehaviour
	{
		// External

		Library Lib;
		
		// Properties

		[SerializeField] WorldPreset preset;
		[SerializeField] RectTransform container;

		[Header("Types")]
			[SerializeField] GameObject discoveryNotif;
			[SerializeField] GameObject exportNotif;

		[Header("Export")] 
			[SerializeField] RectTransform exportRoot;
			
		[Header("Miscellaneous")] 
			[SerializeField] Burster burster;


		void Start()
		{
			Lib = Library.Instance;
			Lib.onDiscoverFile += TriggerDiscoveryNotif;
		}

		void OnDestroy()
		{
			Lib.onDiscoverFile -= TriggerDiscoveryNotif;
		}

		#region Generic
		
		public GenericNotification CreateNotification(GameObject notification, string message, Vector2 position, float lifetime = Mathf.Infinity)
		{
			var instance = Instantiate(notification, container);

			var notif = instance.GetComponent<GenericNotification>();
			if (notif != null) 
			{
				notif.Rect.anchoredPosition = position;
				notif.Initialize(message, lifetime);
				
				return notif;
			}
			else 
			{
				Debug.LogWarning("Attempted to create an invalid notification!");
				Destroy(instance);

				return null;
			}
		}
		
		#endregion
		
		
		
		#region Discoveries
		
		public void TriggerDiscoveryNotif(string filename)
		{
			var containerSize = container.rect.size / 3f;
			var randomPosition = new Vector2(Random.Range(-containerSize.x, containerSize.x), Random.Range(-containerSize.y, containerSize.y));
			var randomLifetime = Random.Range(2f, 4f);

			var notif = CreateNotification(discoveryNotif, filename, randomPosition, lifetime: randomLifetime);
			if (notif != null) 
			{
				burster.Burst(notif.Rect.position);
			}
		}

		public void TriggerExportNotif(string filename)
		{
			var lifetime = 8f;

			var notif = CreateNotification(exportNotif, filename, exportRoot.anchoredPosition, lifetime: lifetime);
			if (notif != null) 
			{
				burster.Burst(notif.Rect.position);
			}
		}
		
		#endregion
	}
}