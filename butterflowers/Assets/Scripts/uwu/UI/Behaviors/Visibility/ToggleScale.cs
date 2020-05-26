using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIExt.Behaviors.Visibility {

    public class ToggleScale : ToggleVisibility
    {
        RectTransform rect;

        [SerializeField] protected Vector2 visible, hidden;
                         protected Vector2 current, target;

        private void Awake() {
            rect = GetComponent<RectTransform>();
        }

        protected override IEnumerator UpdatingVisibility(){
            float dist = Vector3.Distance(current, target);
            while(dist > .033f){
                lerping = true;

                current = Vector3.Lerp(current, target, Time.deltaTime * transitionSpeed);
                EvaluateVisibility();

                dist = Vector3.Distance(current, target);
                yield return null;
            }

            current = target;
            EvaluateVisibility();
            lerping = false;
        }

        protected override void EvaluateVisibility(){
            rect.localScale = new Vector3(current.x, current.y, 1f);
        }

        protected override void SetTarget(bool isVisible){ target = (isVisible)? visible:hidden; }
        protected override void SetCurrent(bool isVisible){ current = (isVisible)? visible:hidden; }

        protected override void SetTargetToCurrent(){ target = current; }
        protected override void SetCurrentToTarget(){ current = target; }
    }

}