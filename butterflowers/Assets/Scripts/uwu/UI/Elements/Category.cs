using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UIExt.Behaviors.Visibility;

namespace UIExt.Elements {

    public class Category : MonoBehaviour {
                        ToggleOpacity opacity;
        [SerializeField] TogglePosition options;

        void Awake() {
            opacity = GetComponent<ToggleOpacity>();    
        }

        public void Show(){
            opacity.Show();
            options.Show();
        }

        public void Hide(){
            opacity.Hide();
            options.Hide();
        }
    }

}
