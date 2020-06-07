using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using UnityScript.Steps;

using Noder;

namespace Wizard {

    public class Dialogue: DialogueHandler {

        #region Properties

        [SerializeField] Noder.Graphs.DialogueTree DialogueTree;

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

        void Start()
        {
            DialogueTree.dialogueHandler = this;
        }

        void OnEnable()
        {
            DialogueTree.onUpdateNode += onUpdateNode;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) DialogueTree.Step();
        }

		#endregion

		#region Operations

		public void FetchDialogueFromTree() {
            DialogueTree.Step();
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

        #region Graph callbacks

        void onUpdateNode(Node node)
        {
            
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
