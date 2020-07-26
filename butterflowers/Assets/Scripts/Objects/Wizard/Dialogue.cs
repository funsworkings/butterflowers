using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UIExt.Behaviors.Visibility;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using UnityScript.Steps;

using Noder;
using Noder.Nodes.External;
using System.Linq;
using Noder.Graphs;
using System.Text;

namespace Wizard {

    using Mood = Brain.MoodState;
    using Stance = Brain.StanceState;

    public class Dialogue: DialogueHandler {

        World world;

        #region Properties

        [SerializeField] DialogueTree[] dialogueTrees;
        [SerializeField] DialogueTree coreDialogueTree;

        Controller controller;
        Memories memories;
        Brain brain;

        [SerializeField] Text_WiggleEffect memoryTextEffect;

        [SerializeField] ToggleOpacity bubble = null;
        [SerializeField] TogglePosition bubble_pos = null;
        [SerializeField] ToggleOpacity advancer = null;

        [SerializeField] GameObject alert;

        #endregion

        #region Internal

        [System.Serializable]
        public struct CommentMapping 
        {
            public Mood mood;
            public Stance stance;
            public DialogueCollection dialogue;
        }

        [System.Serializable]
        public struct ReactionMapping 
        {
            public EVENTCODE @event;
            public Mood mood;
            public DialogueCollection dialogue;
        }

		#endregion

		#region Collections

		[SerializeField] List<string> temp = new List<string>();
        [SerializeField] List<int> tempvisited = new List<int>();
        [SerializeField] string m_comment = null;

        #endregion

        #region Attributes

        public readonly string imageFlag = ":i:";
        public readonly string memoryFlag = ":m:";
        public readonly string daysFlag = ":d:";
        public readonly string eventFlag = ":e:";

        [SerializeField] int node_id = -1;

        [SerializeField] [Range(0f, 1f)] float m_minRandomness = .1f, m_maxRandomness = 1f;
        [SerializeField] float m_minSymbolDelay = 0f, m_maxSymbolDelay = 1f;

        [SerializeField] CommentMapping[] commentaries = new CommentMapping[] { };
        [SerializeField] ReactionMapping[] reactions = new ReactionMapping[] { };

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
                var node = coreDialogueTree.GetNodeByInstanceId(node_id);
                coreDialogueTree.activeNode = node;
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
                coreDialogueTree.FlagTemporaryDialogue(value);
            }
        }

        public string[] temporaryqueue => temp.ToArray();
        public bool waiting => temp.Count > 0 || !string.IsNullOrEmpty(m_comment);

        [SerializeField] string[] debugBodies = new string[] { };

        public bool isAlertActive => alert.activeSelf;

        public string comment {
            set
            {
                bool alerts = !string.IsNullOrEmpty(value);
                m_comment = value;

                alert.SetActive(alerts);
            }
        }

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            controller = GetComponent<Controller>();
            brain = GetComponent<Brain>();
        }

        void Start()
        {
            memories = controller.Memories;

            foreach (DialogueTree tree in dialogueTrees)
                tree.dialogueHandler = this;
        }

        void OnEnable()
        {
            world = World.Instance;

            coreDialogueTree.onUpdateNode += onUpdateNode;
            DialogueTree.onReceiveDialogue += onReceiveDialogue;
        }

        void OnDisable()
        {
            coreDialogueTree.onUpdateNode -= onUpdateNode;
            DialogueTree.onReceiveDialogue -= onReceiveDialogue;
        }

        void OnDestroy()
        {
            foreach (DialogueTree tree in dialogueTrees)
                tree.Dispose();
        }

        #endregion

        #region Operations

        public void PushAllFromQueue()
        {
            Push(m_comment);
            m_comment = null;

            for (int i = 0; i < temp.Count; i++) 
            {
                Push(temp[i]);
            }
            temp = new List<string>();
        }

        public void FetchDialogueFromTree(DialogueTree tree = null)
        {
            if (tree == null)
                tree = coreDialogueTree;

            tree.Step();
        }

        public string FetchDialogueFromTree(int value, DialogueTree tree = null)
        {
            if (tree == null)
                tree = coreDialogueTree;

            tree.Step(value);

            var node = tree.activeNode;
            string body = (node != null) ? (node as BaseDialogueNode).body : "";

            return body;
        }

        public void React(EVENTCODE @event, Mood mood)
        {
            var mappings = reactions.Where(r => (r.@event == @event && (r.mood == Mood.NULL || r.mood == mood)) );
            if (mappings.Count() == 0) {
                Debug.LogErrorFormat("Found no matching reactions for event = {0} mood = {1}", @event, mood);
                return;
            }

            var map = mappings.First(); // Get first mapping
            var collection = map.dialogue;

            string reaction = collection.FetchRandomItem();
            Push(reaction, true);
        }

        public void Comment(Mood mood, Stance stance)
        {
            if (commentaries.Length == 0) return;

            var mappings = commentaries.Where(c => c.stance == stance && c.mood == mood);
            if (mappings.Count() == 0) {
                Debug.LogErrorFormat("Unable to find commentary mapping for ->  mood = {0} stance = {1}", mood, stance);
                return;
            }

            var mapping = mappings.First();
            var collection = mapping.dialogue;

            string comment = collection.FetchRandomItem();
            if (controller.isFocused) 
            {
                Push(comment, true);
                this.m_comment = null;
            }
            else
                this.m_comment = comment;
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

        public override void Dispose()
        {
            base.Dispose();

            temp = new List<string>();
        }

        protected override string ParseBody(string body)
        {
            if (string.IsNullOrEmpty(body))
                return body;

            body = Extensions.ReplaceEnclosingPattern(body, imageFlag, "<sprite name=\"{0}\">"); // Replace img tag
            body = body.Replace(daysFlag, Sun.Instance.days.ToString());

            var @event = Events.LAST_EVENT_DETAIL;
            body = body.Replace(eventFlag, @event);

            return body;
        }

        protected override void OnSpeak()
        {
            base.OnSpeak();

            bubble.Show();
            bubble_pos.Show();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            bubble.Hide();
            bubble_pos.Hide();

            advancer.Hide();
        }

        protected override void OnStart(string body)
        {
            bool found = DiscoveryMemoriesFromBody(body);
            m_body = FilterMemories(body, found);

            if(!debug) UpdateTimeBetweenSymbols();

            memoryTextEffect.magnitude = (found)? 8f:0f;
            advancer.Hide();
        }

        /* previous focus dialogue behaviour
         * 
         * autoprogress = false;
                    advancer.Show();

                    if (!available) {
                        var mood = Mathf.RoundToInt(brain.mood);
                        FetchDialogueFromTree(mood, focusDialogueTree);

                        controller.SetAbsorbState(false);
                    }
         * 
         * 
         */

        protected override void OnComplete(string body)
        {
            if (world.STATE == GAMESTATE.GAME) 
            {
                autoprogress = true;
                advancer.Hide();
            }
            else 
                advancer.Show();
        }

        #endregion

        #region Graph callbacks

        void onUpdateNode(Node node)
        {
            int instance_id = (node == null)? -1:node.GetInstanceID();
            //Debug.Log("node = " + instance_id);

            if (node is TemporaryDialogue) 
            {
                if (!tempvisited.Contains(instance_id)) {
                    tempvisited.Add(instance_id);

                    controller.UpdateDialogueVisited(tempvisited.ToArray());
                }
            }

            controller.UpdateCurrentDialogueNode(instance_id);
        }

        void onReceiveDialogue(DialogueTree tree, string body)
        {
            Push(body, true);
        }

        #endregion

        #region Parsing

        string FilterMemories(string body, bool found = false)
        {
            body = Extensions.ReplaceEnclosingPattern(body, memoryFlag, "");
            if (found) body = string.Format("<i><color={0}>{1}", COLOR_LOOKUP.AGENTS[AGENT.Wizard], body); // Italicize memory

            return body;
        }

        bool DiscoveryMemoriesFromBody(string body)
        {
            var memories = ParseMemoriesFromBody(body);
            Debug.LogFormat("FOUND {0} MEMORIES FROM BODY={1}", memories.Length, body);

            for (int i = 0; i < memories.Length; i++)
                controller.DiscoverMemory(memories[i]);

            return (memories.Length > 0);
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
