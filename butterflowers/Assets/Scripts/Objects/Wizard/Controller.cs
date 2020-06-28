using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Noder.Nodes.Abstract;
using Settings;
using UnityEngine;
using UnityEngine.AI;

namespace Wizard {

    using ActionType = Actions.Type;
    using Action = Actions.Action;
    using ActionSequence = Actions.ActionSequence;

    public class Controller: MonoBehaviour {
        
        #region Events

        public System.Action<Memory> onDiscoverMemory;

		#endregion

		#region External

		GameDataSaveSystem Save;

        [SerializeField] Wand wand;
        [SerializeField] Nest Nest;
        [SerializeField] Focus Focus;
        [SerializeField] Camera Look;

		#endregion

		#region Internal

		public enum State { Idle, Walk, Spell, Rest, Picture }

        #endregion

        #region Properties

        [SerializeField] 
        WorldPreset Preset;

        [SerializeField]
        BrainPreset BrainPreset;

        [SerializeField] 
        Memories m_Memories;

        Animator animator;
        IK ik;

        FocalPoint FocalPoint;
        Brain m_Brain;
        Navigation m_Navigation;
        Actions m_Actions;
        Dialogue m_Dialogue; 
        Audio Audio;

        #endregion

        #region Attributes

        [SerializeField] State state = State.Idle;

        bool load = false;
        bool debug = false;

        [SerializeField] CanvasGroup debugwindow;
        [SerializeField] UnityEngine.UI.Text debugtext;

        [SerializeField] ActionSequence[] sequences;

        #endregion

        #region Accessors

        // Core functionality accessors
        public Brain Brain => m_Brain;
        public Navigation Navigation => m_Navigation;
        public Actions Actions => m_Actions;
        public Dialogue Dialogue => m_Dialogue;
        public Memories Memories => m_Memories;

        public Wand Wand => wand;
        public Animator Animator => animator;

        public bool isFocused {
            get
            {
                if (FocalPoint == null) return false;
                return FocalPoint.isFocused;
            }
        }

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            ik = GetComponentInChildren<IK>();
            animator = GetComponentInChildren<Animator>();

            FocalPoint = GetComponent<FocalPoint>();
            m_Brain = GetComponent<Brain>();
            m_Navigation = GetComponent<Navigation>();
            m_Actions = GetComponent<Actions>();
            m_Dialogue = GetComponent<Dialogue>();
            Audio = GetComponent<Audio>();
        }

        IEnumerator Start()
        {
            Save = GameDataSaveSystem.Instance;

            while (Save == null || !Save.load) yield return null;

            Dialogue.nodeID = (!Preset.persistDialogue)? -1 : Save.dialogueNode;
            Dialogue.nodeIDsVisited = (!Preset.persistDialogueVisited) ? new int[] { } : Save.dialogueVisited;

            var enviro_knowledge = (!Preset.persistKnowledge) ? 0f : Save.enviro_knowledge;
            var file_knowledge = (!Preset.persistKnowledge) ? new Knowledge[] { } : Save.file_knowledge;

            Brain.Load(enviro_knowledge, file_knowledge);

            load = true;
        }

        void OnEnable()
        {
            FocalPoint.onFocus += onFocus;
            FocalPoint.onLoseFocus += onLoseFocus;

            Sun.onDayBegin += onDayNightCycle;
            Sun.onNightBegin += onDayNightCycle;

            Nest.onAddBeacon += onIngestBeacon;

            Actions.onEnact += onEnactAction;

            Events.onFireEvent += onFireEvent;
        }

        void OnDisable()
        {
            FocalPoint.onFocus -= onFocus;
            FocalPoint.onLoseFocus -= onLoseFocus;

            Sun.onDayBegin -= onDayNightCycle;
            Sun.onNightBegin -= onDayNightCycle;

            Nest.onAddBeacon -= onIngestBeacon;

            Actions.onEnact -= onEnactAction;

            Events.onFireEvent -= onFireEvent;
        }

        // Update is called once per frame
        void Update()
        {
            if (!load) return;

            if (FocalPoint.isFocused) 
                if (Input.GetKeyDown(KeyCode.RightArrow)) Dialogue.Advance();

            EvaluateState();
            UpdateAnimatorFromState(state);

            if (Input.GetKeyDown(KeyCode.Z)) debug = !debug;

            debugwindow.alpha = (debug) ? 1f : 0f;
            if (debug) DebugAttributes();

            
            if (Input.GetKeyDown(KeyCode.S))
                Actions.Push(ActionType.Picture, Extensions.RandomString(12), true);
            if (Input.GetKeyDown(KeyCode.D))
                Actions.Push(ActionType.Dialogue);
            if (Input.GetKeyDown(KeyCode.E))
                Actions.Push(ActionType.Emote);
            if (Input.GetKey(KeyCode.N)) {
                if (Input.GetKeyDown(KeyCode.Alpha0)) {
                    Actions.Push(ActionType.NestKick);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    Actions.Push(ActionType.NestPop);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2)) {
                    var opts = Manager.Instance.ActiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.NestPop, beacon);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3)) {
                    Actions.Push(ActionType.NestClear);
                }
            }
            if (Input.GetKey(KeyCode.B)){
                if (Input.GetKeyDown(KeyCode.Alpha0)) {
                    var opts = Manager.Instance.InactiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.BeaconActivate, beacon);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    var opts = Manager.Instance.InactiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.BeaconDestroy, beacon);
                }
            }
            if (Input.GetKeyDown(KeyCode.G)) {
                wand.DoRandomGesture();
            }
            if (Input.GetKeyDown(KeyCode.M)) {
                var opts = Manager.Instance.InactiveBeacons.PickRandomSubset(1);
                var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                if (beacon != null) {
                    var first = new Action();
                    first.type = ActionType.MoveTo;
                    first.dat = beacon.origin;

                    var sec = new Action();
                    sec.type = ActionType.Inspect;
                    sec.dat = beacon.transform;

                    var seq = new ActionSequence();
                    seq.actions = new Action[] { first, sec };
                    seq.immediate = true;

                    Actions.Push(seq);
                }
            }
            
        }

		#endregion

		#region Internal

		void EvaluateState()
        {
            float speed = Navigation.speed;

            //Debug.LogFormat("Nav Agent --- speed = {0}", speed);
            if (speed > .1f) {  // Set to moving state
                state = State.Walk;
            }
            else {  // Set to idle state

                if (state != State.Rest) 
                {
                    if (Wand.spells) 
                        state = State.Spell;
                    else 
                        state = State.Idle;
                }
            }
        }

        void EnactAction()
        {
            var action = Brain.ChooseBestAction();
            if (action != null) 
            {
                Debug.LogFormat("FOUND action => type: {0} dat: {1}", action.type, action.debug);
                Actions.Push(action);
            }
            else {
                Debug.LogFormat("FALLBACK action => NONE");
                //Actions.Push(ActionType.Emote);
            }
        }

        void RespondToPlayerAction(EVENTCODE @event)
        {
            float base_probability = 1f - BrainPreset.actionProbabilityStanceCurve.Evaluate(Brain.stance);
            float min = BrainPreset.minimumResponseActionProbability;
            float max = BrainPreset.maximumResponseActionProbability;

            float prob = Mathf.Clamp(base_probability, min, max);
            if (Random.Range(0f, 1f) <= prob) 
            {
                switch (@event) 
                {
                    case EVENTCODE.BEACONACTIVATE:
                        if (Brain.mood < 0f) Actions.Push(ActionType.NestPop, immediate: true);
                        else Actions.Push(ActionType.BeaconActivate, immediate: true);
                        break;
                    case EVENTCODE.NESTKICK:
                        Actions.Push(ActionType.NestKick, immediate: true);
                        Actions.Push(ActionType.Dialogue, "I like to kick nests too, y'know????", immediate: true);
                        break;
                    case EVENTCODE.NESTSPILL:
                        if (Brain.mood > 0f) Actions.Push(ActionType.BeaconActivate, immediate: true);
                        else Actions.Push(ActionType.NestKick, immediate: true);
                        break;
                    default:
                        break;
                }
            }
        }

		#endregion

		#region Animation

		public void UpdateAnimatorFromState(State state)
        {
            animator.SetBool("walking", state == State.Walk);
            animator.SetBool("resting", state == State.Rest);
            animator.SetBool("spell", state == State.Spell);
        }

		#endregion

        #region Miscellaneous

        public void WakeUp()
        {
            state = State.Idle;
        }

        #endregion

        #region Dialogue

        void onFocus()
        {
            if (Dialogue.queue.Length > 0) {
                Dialogue.autoprogress = true;
                Dialogue.PushAllFromQueue();
            }
            else {
                Dialogue.autoprogress = false;
                Dialogue.FetchDialogueFromTree();
            }
        }

        void onLoseFocus()
        {
            Dialogue.Dispose();
        }

        void OnCompleteDialogue()
        {

        }

        public void UpdateCurrentDialogueNode(int nodeID)
        {
            Save.dialogueNode = nodeID;
        }

        public void UpdateDialogueVisited(int[] nodeIDs)
        {
            Save.dialogueVisited = nodeIDs;
        }

        public void DiscoverMemory(Memory memory)
        {
            Debug.LogFormat("FOUND memory = {0}", memory.name);

            Brain.EncounterMemory(memory, false);
            Audio.PushMemoryAudio(memory.audio);
            Actions.Push(ActionType.Emote, immediate:true); // Immediately emote

            if (onDiscoverMemory != null)
                onDiscoverMemory(memory);
        }

        #endregion

        #region Nest callbacks

        void onIngestBeacon(Beacon beacon)
        {
            var file = beacon.file;
            var mem = Memories.GetMemoryByName(file);

            if (mem != null) Brain.EncounterMemory(mem, true);
        }

		#endregion

		#region Brain

		public void UpdateKnowledgeBase(float enviro, Knowledge[] files)
        {
            Save.enviro_knowledge = enviro;
            Save.file_knowledge = files;
        }

        #endregion

        #region Action callbacks

        void onEnactAction(Action action)
        {
            if (!action.passive) 
            {
                if (isFocused) Focus.LoseFocus();
            }

            if (action.cast) 
                animator.SetTrigger("cast");
        }

        #endregion

        #region Animator callbacks

        public void DidCastSpell()
        {
            Actions.onCastSpell();
        }

        public void DidInspect()
        {
            Actions.onInspect();
        }

		#endregion

		#region Sun callbacks

        void onDayNightCycle() 
        {
            float base_probability = BrainPreset.actionProbabilityStanceCurve.Evaluate(Brain.stance);
            float min = BrainPreset.minimumDayNightActionProbability;
            float max = BrainPreset.maximumDayNightActionProbability;

            float prob = Mathf.Clamp(base_probability, min, max);
            if (Random.Range(0f, 1f) <= prob)
                EnactAction();
        }

        #endregion

        #region Event callbacks

        void onFireEvent(EVENTCODE @event, AGENT a, AGENT b, string detail) 
        {
            if (a != AGENT.Inhabitant0) return; // Ignore all non-player events
            RespondToPlayerAction(@event);
        }

		#endregion

		#region Debug

		void DebugAttributes()
        {
            string debug = "";

            debug += string.Format("STANCE = {0}", Brain.stance);
            debug += string.Format("\nMOOD = {0}", Brain.mood);
            
            debug += "\n\n- - - CURRENT ACTION - - -";

            var currentAction = Actions.currentAction;
            if (currentAction != null) {
                debug += string.Format("\ntype={0} dat={1}", currentAction.type, currentAction.debug);
            }

            debug += "\n\n- - - ACTION QUEUE - - -";

            var queue = Actions.queue;
            foreach (Action a in queue)
                debug += string.Format("\ntype={0} dat={1}", a.type, a.debug);

            debug += "\n- - - END ACTION QUEUE - - -";
            debugtext.text = debug;
        }

        #endregion

    }

}
