using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using UnityScript.Steps;

namespace Wizard {

    public class Dialogue: DialogueHandler {

        #region Properties

        Controller controller;
        [SerializeField] ToggleOpacity bubble = null;

        #endregion

        #region Attributes

        public readonly string imageFlag = ":i:";

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            controller = GetComponent<Controller>();
        }

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

        protected override void OnComplete(string body)
        {
            var memories = ParseMemoriesFromBody(body);
            for (int i = 0; i < memories.Length; i++) {
                //Debug.LogFormat("Dialogue found memory = {0}", memories[i].name);

                controller.CreateBeaconFromMemory(memories[i]);
            }            
        }

        #endregion

        #region Parsing

        Memory[] ParseMemoriesFromBody(string body)
        {
            string patt = "(sprite name=\").*?(\")";
            var matches = Regex.Matches(body, patt);

            List<Memory> parsed = new List<Memory>();
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i].Value;
                match = match.Replace("sprite name=\"", "").Replace("\"", "");

                var mem = controller.Memories.GetMemoryByThumbnail(match);
                if (mem != null) parsed.Add(mem);
            }

            return parsed.ToArray();
        }

		#endregion
	}

}
