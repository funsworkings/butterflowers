using UnityEngine;
using UnityEngine.Events;

namespace uwu.Snippets
{
	[RequireComponent(typeof(Collider))]
	public class ForwardCollisionEvents : MonoBehaviour
	{
		public UnityEvent onEnter, onExit;

		void OnCollisionEnter(Collision col)
		{
			onEnter.Invoke();
		}

		void OnCollisionExit(Collision col)
		{
			onExit.Invoke();
		}

		void OnTriggerEnter(Collider col)
		{
			onEnter.Invoke();
		}

		void OnTriggerExit(Collider col)
		{
			onExit.Invoke();
		}
	}
}