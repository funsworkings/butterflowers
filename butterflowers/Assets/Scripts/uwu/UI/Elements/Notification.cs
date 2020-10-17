using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using uwu.UI.Behaviors.Visibility;

namespace uwu.UI.Elements
{
	public class Notification : MonoBehaviour
	{
		public UnityEvent OnNotify;

		[SerializeField] float lifetime = .33f; // How long to stay on screen for after SHOW
		bool notify;

		ToggleOpacity opacity;

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		public void Show()
		{
			if (notify)
				StopCoroutine("Showing");

			notify = true;
			StartCoroutine("Showing");

			OnNotify.Invoke();
		}

		IEnumerator Showing()
		{
			opacity.SetOpacity(1f);
			yield return new WaitForSeconds(lifetime);
			opacity.Hide();
		}
	}
}