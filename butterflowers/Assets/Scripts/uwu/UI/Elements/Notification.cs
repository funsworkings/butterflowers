using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UIExt.Behaviors.Visibility;

namespace UIExt.Elements {

    public class Notification : MonoBehaviour
    {   
        public UnityEvent OnNotify;

        ToggleOpacity opacity;

        [SerializeField] float lifetime = .33f; // How long to stay on screen for after SHOW
                         bool notify = false;

        void Awake() {
            opacity = GetComponent<ToggleOpacity>();    
        }

        public void Show(){
            if(notify)
                StopCoroutine("Showing");
            
            notify = true;
            StartCoroutine("Showing");

            OnNotify.Invoke();
        }

        IEnumerator Showing(){
            opacity.SetOpacity(1f);
            yield return new WaitForSeconds(lifetime);
            opacity.Hide();
        }
    }

}
