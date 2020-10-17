using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uwu.UI.Behaviors;

namespace uwu.UI.Elements
{
	public class Page : MonoBehaviour
	{
		public UnityEvent onOpen, onClose;

		[SerializeField] List<GameObject> objects = new List<GameObject>();

		Section section;

		ToggleVisibility visibility;

		public bool active { get; set; }

		void Awake()
		{
			section = GetComponentInParent<Section>();
			visibility = GetComponent<ToggleVisibility>();

			var children = transform.childCount;
			for (var i = 0; i < children; i++) {
				var obj = transform.GetChild(i).gameObject;
				objects.Add(obj);
			}

			//Debug.LogFormat("Page \"{0}\" has {1} children", gameObject.name, children);
		}

		void Start()
		{
			active = false;
		}

		public void Open(bool events = false)
		{
			var inactive = !active;

			active = true;
			onUpdateActiveState();

			if (inactive || events)
				onOpen.Invoke();
		}

		public void Close(bool events = false)
		{
			var inactive = !active;

			active = false;
			if (!inactive || events)
				onClose.Invoke();

			onUpdateActiveState();
		}

		void onUpdateActiveState()
		{
			foreach (var o in objects) o.SetActive(active);
		}
	}
}