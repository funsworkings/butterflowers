using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;
using System.Text.RegularExpressions;

namespace Wizard {

    public class Dialogue: DialogueHandler {

        #region Properties

        [SerializeField] ToggleOpacity bubble = null;

        #endregion

        #region Attributes

        public readonly string imageFlag = ":i:";

		#endregion

		#region Dialogue overrides

		protected override string ParseBody(string body)
        {
            body = Extensions.ReplaceEnclosingPattern(body, imageFlag, "<sprite name=\"{0}\">"); // Replace img tag

            return body;
        }

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

        #endregion

        #region Parsing

        string InsertFromImageFlag(string body)
        {
            string pattern = string.Format("{0}.*?{0}", imageFlag);
            Debug.Log((Regex.Matches(body, pattern)).Count);

            return Regex.Replace(body, pattern, ParseImageFromFlag);
        }

        string ParseImageFromFlag(Match match) 
        {
            var contents = match.Value;
            contents = contents.Replace(imageFlag, "");

            return string.Format("<sprite name=\"{0}\">", contents.Trim());
        }

		#endregion
	}

}
