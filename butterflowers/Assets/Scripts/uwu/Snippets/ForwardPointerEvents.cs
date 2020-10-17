using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace uwu.Snippets
{
	public class ForwardPointerEvents : MonoBehaviour
		, IPointerDownHandler
		, IPointerEnterHandler
		, IPointerExitHandler
		, IPointerUpHandler
	{
		public UnityEvent onEnter, onExit, onDown, onUp;
		public bool debug;

		public void OnPointerDown(PointerEventData dat)
		{
			onDown.Invoke();
			if (debug) Debug.Log(gameObject.name + " was down.");
		}

		public void OnPointerEnter(PointerEventData dat)
		{
			onEnter.Invoke();
			if (debug) Debug.Log(gameObject.name + " was entered.");
		}

		public void OnPointerExit(PointerEventData dat)
		{
			onExit.Invoke();
			if (debug) Debug.Log(gameObject.name + " was exited.");
		}

		public void OnPointerUp(PointerEventData dat)
		{
			onUp.Invoke();
			if (debug) Debug.Log(gameObject.name + " was up.");
		}
	}
}