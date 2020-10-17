using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace uwu.UI.Elements
{
	public class ActionBlock : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] protected bool internalEvents = true;
		public Action onInvokeAction;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!internalEvents)
				return;

			InvokeAction();
			if (onInvokeAction != null)
				onInvokeAction();
		}

		public void EnactAction()
		{
			if (internalEvents)
				return;

			InvokeAction();
			if (onInvokeAction != null)
				onInvokeAction();
		}

		protected virtual void InvokeAction()
		{
		}
	}
}