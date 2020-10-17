using UnityEngine;
using UnityEngine.Events;

namespace uwu.Events
{
	public class EnableEvents : MonoBehaviour
	{
		public UnityEvent Enabled, Disabled;

		void OnEnable()
		{
			Enabled.Invoke();
		}

		void OnDisable()
		{
			Disabled.Invoke();
		}
	}
}