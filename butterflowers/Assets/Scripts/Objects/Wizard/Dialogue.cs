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
        Memories memories;

        [SerializeField] GameObject alert = null;
        [SerializeField] ToggleOpacity bubble = null;

        #endregion

        #region Collections

        [SerializeField] List<string> temp = new List<string>();

        Dictionary<string, Memory[]> mems = new Dictionary<string, Memory[]>();

		#endregion

		#region Attributes

		public readonly string imageFlag = ":i:";
        public readonly string memoryFlag = ":m:";

        [SerializeField] int node_id = -1;

		#endregion

		#region Accessors

        public int nodeID {
            get
            {
                return node_id;
            }
            set
            {
                bool refresh = (node_id != value);
                node_id = value;

                Debug.LogFormat("Attempt set {0} as node", node_id);
                if (refresh) {
                    var node = DialogueTree.GetNodeByInstanceId(node_id);
                    if (node == null) node = DialogueTree.rootNode;

                    DialogueTree.activeNode = node;
                }
            }
        }

		#endregion

		#region Monobehaviour callbacks

		void Awake()
        {
            controller = GetComponent<Controller>();
        }

        void Start()
        {
            memories = controller.Memories;
            DialogueTree.dialogueHandler = this;
        }

        void OnEnable()
        {
            DialogueTree.onUpdateNode += onUpdateNode;
        }

        void Update()
        {
            alert.SetActive(temp.Count > 0);
        }

        void OnDestroy()
        {
            DialogueTree.Dispose();
        }

        #endregion

        #region Operations

        public void PushAllFromQueue()
        {
            for (int i = 0; i < temp.Count; i++) {
                Push(temp[i]);
            }
            temp = new List<string>();
        }

		public void FetchDialogueFromTree() {
            DialogueTree.Step();
        }

        public void FetchDialogueFromTree(int value)
        {
            DialogueTree.Step(value);
        }

        #endregion

        #region Dialogue overrides

        public override void Push(string body)
        {
            if (controller.isFocused) 
            {
                AddMemoriesFromBody(body);   
                base.Push(body);
            }
            else 
            {
                temp.Add(body); // Store temporarily 
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            temp = new List<string>();
            mems.Clear();
        }

        protected override string ParseBody(string body)
        {
            body = Extensions.ReplaceEnclosingPattern(body, imageFlag, "<sprite name=\"{0}\">"); // Replace img tag
            body = Extensions.ReplaceEnclosingPattern(body, memoryFlag, "");

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
            var memories = mems[body];
            if (memories != null) {
                for (int i = 0; i < memories.Length; i++) {
                    Debug.LogFormat("Dialogue found memory = {0}", memories[i].name);
                    controller.CreateBeaconFromMemory(memories[i]);
                }
                mems.Remove(body); // Remove item from dictionary
            }
        }

        #endregion

        #region Graph callbacks

        void onUpdateNode(Node node)
        {
            if (node == null) return;

            int instance_id = node.GetInstanceID();
            Debug.Log("node = " + instance_id);

            controller.UpdateCurrentDialogueNode(instance_id);
        }

        #endregion

        #region Parsing

        void AddMemoriesFromBody(string body)
        {
            var memories = ParseMemoriesFromBody(body);
            var parsed = ParseBody(body);

            if (!mems.ContainsKey(parsed)) {
                mems.Add(parsed, memories);
            }
            else {
                mems[parsed] = memories;
            }
        }

		Memory[] ParseMemoriesFromBody(string body)
        {
            string patt = "(:m:).*?(:m:)";
            var matches = Regex.Matches(body, patt);

            List<Memory> parsed = new List<Memory>();
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i].Value;
                match = match.Replace(":m:", "");

                var mem = controller.Memories.GetMemoryByName(match);
                if (mem != null) parsed.Add(mem);
            }

            return parsed.ToArray();
        }

		#endregion
	}

}
