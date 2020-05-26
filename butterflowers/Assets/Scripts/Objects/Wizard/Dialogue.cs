using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;

namespace Wizard {

    public class Dialogue: DialogueHandler {

        #region Properties

        [SerializeField] ToggleOpacity bubble = null;

        #endregion

        protected override void OnSpeak()
        {
            base.OnSpeak();
            bubble.Show();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            bubble.Hide();
        }
    }

}
