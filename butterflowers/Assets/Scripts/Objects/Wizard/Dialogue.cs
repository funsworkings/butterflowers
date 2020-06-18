using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using UnityScript.Steps;

using Noder;
using Noder.Nodes.External;

namespace Wizard {

    public class Dialogue: DialogueHandler {

        #region Properties

        [SerializeField] Noder.Graphs.DialogueTree DialogueTree;

        Controller controller;
        Memories memories;
        Brain brain;

        [SerializeField] GameObject alert = null;
        [SerializeField] ToggleOpacity bubble = null;
        [SerializeField] ToggleOpacity advancer = null;

        #endregion

        #region Collections

        [SerializeField] List<string> temp = new List<string>();
        [SerializeField] List<int> tempvisited = new List<int>();

        #endregion

        #region Attributes

        public readonly string imageFlag = ":i:";
        public readonly string memoryFlag = ":m:";
        public readonly string daysFlag = ":d:";

        [SerializeField] int node_id = -1;

        [SerializeField] [Range(0f, 1f)] float m_minRandomness = .1f, m_maxRandomness = 1f;
        [SerializeField] float m_minSymbolDelay = 0f, m_maxSymbolDelay = 1f;

        #endregion

        #region Accessors

        public float minRandomness => m_minRandomness;
        public float maxRandomness => m_maxRandomness;
        public float minSymbolDelay => m_minSymbolDelay;
        public float maxSymbolDelay => m_maxSymbolDelay;

        public int nodeID {
            get
            {
                return node_id;
            }
            set
            {
                node_id = value;

                Debug.LogFormat("Attempt set {0} as node", node_id);
                var node = DialogueTree.GetNodeByInstanceId(node_id);
                DialogueTree.activeNode = node;
            }
        }

        public int[] nodeIDsVisited {
            get
            {
                return tempvisited.ToArray();
            }
            set
            {
                tempvisited = new List<int>(value);
                DialogueTree.FlagTemporaryDialogue(value);
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
            brain = controller.Brain;

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

        [SerializeField] bool debug = false;
        [SerializeField] [Range(0f, 1f)] float m_randomBetweenSymbols = 0f, m_anchorBetweenSymbols = .5f;
        [SerializeField] float m_rangeBetweenSymbols = 0f, m_minBetweenSymbols = 0f, m_maxBetweenSymbols = 0f;

        public override float timeBetweenSymbols {
            get
            {
                if (debug) 
                {
                    float range = m_randomBetweenSymbols * (maxSymbolDelay - minSymbolDelay);
                    float range_2 = range / 2f;
                    float center = m_anchorBetweenSymbols.RemapNRB(0f, 1f, minSymbolDelay, maxSymbolDelay);

                    float min = center - range_2;
                    float max = center + range_2;

                    var loffset = -Mathf.Max(0f, minSymbolDelay - min);
                    var roffset = Mathf.Min(0f, maxSymbolDelay - max);

                    min += (loffset + roffset);
                    max += (loffset + roffset);

                    Debug.LogFormat("mood={0} stance={1}  random={2}  range={3}  min={4}  max={5}", 0f, 0f, m_randomBetweenSymbols, range, min, max);

                    return Random.Range(min, max);
                }

                return Random.Range(m_minBetweenSymbols, m_maxBetweenSymbols);
            }
        }

        protected void UpdateTimeBetweenSymbols()
        {
            float mood = brain.mood;
            float stance = brain.stance;

            float random = (1f - stance).RemapNRB(0f, 1f, minRandomness, maxRandomness); // Inverse mood 

            float dist = (maxSymbolDelay - minSymbolDelay);
            float range = dist * random;
            float range_2 = range / 2f;
            float center = (-mood).RemapNRB(-1f, 1f, minSymbolDelay, maxSymbolDelay);

            float min = center - range_2;
            float max = center + range_2;

            var loffset = -Mathf.Max(0f, minSymbolDelay - min);
            var roffset = Mathf.Min(0f, maxSymbolDelay - max);

            min += (loffset + roffset);
            max += (loffset + roffset);

            //Debug.LogFormat("mood={0} stance={1}  random={2}  range={3}  min={4}  max={5}", mood, stance, random, range, min, max);
            m_randomBetweenSymbols = random;
            m_rangeBetweenSymbols = range;

            m_minBetweenSymbols = min;
            m_maxBetweenSymbols = max;
        }

        public override void Push(string body)
        {
            if (controller.isFocused) 
            {
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
        }

        protected override string ParseBody(string body)
        {
            body = Extensions.ReplaceEnclosingPattern(body, imageFlag, "<sprite name=\"{0}\">"); // Replace img tag
            body = body.Replace(daysFlag, Sun.Instance.days.ToString());

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
            advancer.Hide();
        }

        protected override void OnStart(string body)
        {
            DiscoveryMemoriesFromBody(body);
            m_body = FilterMemories(body);

            if(!debug) UpdateTimeBetweenSymbols();

            advancer.Hide();
        }

        protected override void OnComplete(string body)
        {
            if (controller.isFocused) {
                FetchDialogueFromTree();
                advancer.Show();
            }
        }

        #endregion

        #region Graph callbacks

        void onUpdateNode(Node node)
        {
            int instance_id = (node == null)? -1:node.GetInstanceID();
            Debug.Log("node = " + instance_id);

            if (node is TemporaryDialogue) 
            {
                if (!tempvisited.Contains(instance_id)) {
                    tempvisited.Add(instance_id);

                    controller.UpdateDialogueVisited(tempvisited.ToArray());
                }
            }

            controller.UpdateCurrentDialogueNode(instance_id);
        }

        #endregion

        #region Parsing

        string FilterMemories(string body)
        {
            body = Extensions.ReplaceEnclosingPattern(body, memoryFlag, "");
            return body;
        }

        void DiscoveryMemoriesFromBody(string body)
        {
            var memories = ParseMemoriesFromBody(body);
            for (int i = 0; i < memories.Length; i++)
                controller.DiscoverMemory(memories[i]);
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
