using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIExt.Behaviors.Visibility
{
    public class ToggleOpacity : ToggleVisibility
    {
        public enum Type {
            CanvasGroup,
            Image,
            Text
        }
        public Type type;

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image image;
        [SerializeField] Text text;


        [SerializeField] protected float visible, hidden;
                         protected float current, target;


        private void Awake() {
            if(type == Type.CanvasGroup && canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            else if(type == Type.Image && image == null)
                image = GetComponent<Image>();
            else if(type == Type.Text && text == null)
                text = GetComponent<Text>();
        }

        protected override IEnumerator UpdatingVisibility(){
            float dist = Mathf.Abs(target - current);
            while(dist > .033f){
                lerping = true;

                current = Mathf.Lerp(current, target, Time.deltaTime * transitionSpeed);
                EvaluateVisibility();

                dist = Mathf.Abs(target - current);
                yield return null;
            }

            current = target;
            EvaluateVisibility();
            lerping = false;
        }

        public void SetOpacity(float alpha){
            current = Mathf.Clamp01(alpha); //Ensure 0-1
            shown = (current > hidden);

            EvaluateVisibility();
        }

        protected override void EvaluateVisibility(){
            if(type == Type.CanvasGroup){
                canvasGroup.alpha = current;
                canvasGroup.interactable = (current > 0f);
                canvasGroup.blocksRaycasts = (current > 0f);      
            } 
            else if(type == Type.Image)
                image.color = Extensions.SetOpacity(current, image.color);
            else if(type == Type.Text)
                text.color = Extensions.SetOpacity(current, text.color);
        }


        protected override void SetTarget(bool isVisible){ target = (isVisible)? visible:hidden; }
        protected override void SetCurrent(bool isVisible){ current = (isVisible)? visible:hidden; }

        protected override void SetTargetToCurrent(){ target = current; }
        protected override void SetCurrentToTarget(){ current = target; }
    }
}

