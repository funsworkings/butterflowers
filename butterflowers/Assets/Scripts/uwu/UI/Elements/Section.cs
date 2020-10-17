using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace uwu.UI.Elements
{
	public class Section : MonoBehaviour
	{
		public UnityEvent onOpen, onClose;

		[SerializeField] bool m_active;

		[SerializeField] Page[] pages;

		public Page defaultPage;

		[SerializeField] List<GameObject> objects = new List<GameObject>();
		Page currentPage, queuePage;

		public bool active
		{
			get => m_active;
			set => m_active = value;
		}

		public Page fetch
		{
			get
			{
				if (queuePage == null)
					return defaultPage;

				return queuePage;
			}
		}

		public Page previous { get; set; }

		public Page current
		{
			get => currentPage;
			set
			{
				if (current != value) {
					previous = current;
					currentPage = value;

					onUpdateActivePage();
				}
			}
		}

		void Awake()
		{
			var pages = 0;

			var children = transform.childCount;
			for (var i = 0; i < children; i++) {
				var obj = transform.GetChild(i).gameObject;
				if (obj.GetComponent<Page>() == null)
					objects.Add(obj);
				else
					pages++;
			}

			//Debug.LogFormat("Section \"{0}\" has {1} children and {2} pages", gameObject.name, children, pages);
		}

		// Start is called before the first frame update
		void Start()
		{
			pages = GetComponentsInChildren<Page>(); // Fetch all pages

			// Ensure defaults are set at beginning
			previous = null;
			currentPage = null;

			if (active) {
				active = false;
				Open();
			}
			else {
				active = true;
				Close();
			}
		}

		void onUpdateActivePage(bool events = false)
		{
			Page activating = null;

			foreach (var page in pages)
				if (page == current)
					activating = page;
				else
					page.Close(events);

			if (activating != null)
				activating.Open(events);
		}

		public void ReturnToPreviousPage()
		{
			if (previous == null) {
				Debug.LogWarning("No previous page to return to!");
				return;
			}

			current = previous;
		}

		public void MoveToPage(Page page)
		{
			if (!ContainsPage(page))
				return;

			current = page;
		}

		public void OpenToPage(Page page)
		{
			if (!ContainsPage(page))
				return;

			OpenToPage_sealed(page);
		}

		void OpenToPage_sealed(Page page, bool self = true)
		{
			if (page == null) {
				Debug.LogWarning("Attempt to open to null page, ignore...");
				return;
			}

			if (active) {
				current = page; // Jump to next page 
			}
			else {
				queuePage = page;
				Open(self);
			}
		}

		public void Open(bool self = true)
		{
			if (self) return;

			var inactive = !active;

			active = true;
			onUpdatedActiveState();

			if (inactive)
				onOpen.Invoke();
		}

		public void Close(bool self = true)
		{
			if (self) return;

			var inactive = !active;

			active = false;
			onUpdatedActiveState();

			if (!inactive)
				onClose.Invoke();
		}

		void onUpdatedActiveState()
		{
			//Debug.Log(gameObject.name + " was made " + active);
			if (active) {
				currentPage = fetch; // Fetch page from page in queue
			}
			else {
				currentPage = null;
				queuePage = null;
			}

			onUpdateActivePage(true);
			foreach (var o in objects)
				o.SetActive(active);
		}

		#region Helpers

		public bool ContainsPage(Page page)
		{
			return page != null && pages.Contains(page);
		}

		#endregion
	}
}