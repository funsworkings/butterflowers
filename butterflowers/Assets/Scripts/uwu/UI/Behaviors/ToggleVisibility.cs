using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

namespace UIExt.Behaviors
{
    /*
    
        Wrapper class for visible elements in UI
            Hide, Show, Toggle

     */

    public abstract class ToggleVisibility : MonoBehaviour
    {
        [SerializeField] protected bool shown = false;
        public bool Shown
        {
            get
            {
                return shown;
            }
            set
            {
                shown = value;
            }
        }

        [SerializeField] protected bool lerps = false;
        protected bool lerping = false;

        [SerializeField] protected float value = 0f;
        [SerializeField] protected float transitionInSpeed = 0f, transitionOutSpeed = 0f;
        protected float transitionSpeed;

        public UnityEvent OnShow, OnHide, OnToggle;

        void OnEnable()
        {
            SetCurrent(shown);
            SetTargetToCurrent();

            EvaluateVisibility();
        }

        void OnDisable()
        {
            if (lerping)
            {
                StopCoroutine("UpdatingVisibility");
                SetCurrentToTarget();
            }
        }

        protected virtual void OnUpdateVisibility()
        {
            if (lerps)
            {
                if (lerping) StopCoroutine("UpdatingVisibility");

                transitionSpeed = (shown) ? transitionInSpeed : transitionOutSpeed;

                if (gameObject.activeInHierarchy)
                    StartCoroutine("UpdatingVisibility");
                else
                    SetCurrentToTarget();
            }
            else
            {
                SetCurrentToTarget();
                EvaluateVisibility();
            }
        }

        protected virtual IEnumerator UpdatingVisibility()
        {
            yield return null;
        }

        protected virtual void EvaluateVisibility() { }

        protected abstract void SetTarget(bool isVisible);
        protected abstract void SetCurrent(bool isVisible);

        protected abstract void SetTargetToCurrent();
        protected abstract void SetCurrentToTarget();

        public void Show()
        {
            if (shown) return;

            shown = true;
            SetTarget(true);

            OnUpdateVisibility();
            OnShow.Invoke();
        }

        public void Hide()
        {
            if (!shown) return;

            shown = false;
            SetTarget(false);

            OnUpdateVisibility();
            OnHide.Invoke();
        }

        public void Toggle()
        {
            shown = !shown;
            SetTarget(shown);

            OnUpdateVisibility();
            OnToggle.Invoke();
        }
    }

}


