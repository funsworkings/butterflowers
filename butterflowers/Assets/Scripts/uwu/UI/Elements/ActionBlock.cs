using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace UIExt.Elements
{

    public class ActionBlock : MonoBehaviour, IPointerClickHandler
    {
        public System.Action onInvokeAction;

        [SerializeField] protected bool internalEvents = true;

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

        protected virtual void InvokeAction() { }
    }

}
