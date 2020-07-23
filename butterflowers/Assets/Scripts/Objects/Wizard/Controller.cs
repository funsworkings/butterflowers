using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Noder.Graphs;
using Noder.Nodes.Abstract;
using Settings;
using UnityEngine;
using UnityEngine.AI;

namespace Wizard {

    using ActionType = Actions.Type;
    using Action = Actions.Action;
    using ActionSequence = Actions.ActionSequence;

    using Mood = Brain.MoodState;
    using Stance = Brain.StanceState;

    public class Controller: MonoBehaviour {
        
        #region Events

        public System.Action<Memory> onDiscoverMemory;

		#endregion

		#region External

		GameDataSaveSystem Save;

        [SerializeField] World World;
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

        Brain m_Brain;
        Navigation m_Navigation;
        Actions m_Actions;
        Dialogue m_Dialogue; 
        Audio Audio;

        SkinnedMeshRenderer Renderer;
        Material DefaultMaterial;

        #endregion

        #region Attributes

        public State state = State.Idle;

        bool load = false;
        bool debug = false;

        [SerializeField] CanvasGroup debugwindow;
        [SerializeField] UnityEngine.UI.Text debugtext;

        [SerializeField] ActionSequence[] sequences;

        [SerializeField] DialogueTree introDialogueTree;

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
                return Focus.active;
            }
        }

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            ik = GetComponentInChildren<IK>();
            animator = GetComponentInChildren<Animator>();

            m_Brain = GetComponent<Brain>();
            m_Navigation = GetComponent<Navigation>();
            m_Actions = GetComponent<Actions>();
            m_Dialogue = GetComponent<Dialogue>();
            Audio = GetComponent<Audio>();

            Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            DefaultMaterial = Renderer.sharedMaterial;
        }

        IEnumerator Start()
        {
            Save = GameDataSaveSystem.Instance;

            while (!World.LOAD)
                yield return null;

            Dialogue.nodeID = (!Preset.persistDialogue)? -1 : Save.dialogueNode;
            Dialogue.nodeIDsVisited = (!Preset.persistDialogueVisited) ? new int[] { } : Save.dialogueVisited;

            var enviro_knowledge = (!Preset.persistKnowledge) ? 0f : Save.enviro_knowledge;
            var file_knowledge = (!Preset.persistKnowledge) ? new Knowledge[] { } : Save.file_knowledge;

            Brain.Load(enviro_knowledge, file_knowledge);

            StartCoroutine("Absorbing");

            load = true;
        }

        void OnDestroy()
        {
            StopCoroutine("Absorbing");
        }

        void OnEnable()
        {
            World = World.Instance;
            World.UPDATE_GAMESTATE += onUpdateGameState;

            Sun.onDayBegin += onDayNightCycle;
            Sun.onNightBegin += onDayNightCycle;

            Nest.onAddBeacon += onIngestBeacon;

            Actions.onEnact += onEnactAction;
            Brain.onUpdateState += onUpdateBrainState;

            Events.onFireEvent += onFireEvent;
            Events.onGlobalEvent += onGlobalEvent;

            DialogueTree.onCompleteTree += onCompleteDialogueTree;
        }

        void OnDisable()
        {
            World.UPDATE_GAMESTATE -= onUpdateGameState;

            Sun.onDayBegin -= onDayNightCycle;
            Sun.onNightBegin -= onDayNightCycle;

            Nest.onAddBeacon -= onIngestBeacon;

            Actions.onEnact -= onEnactAction;
            Brain.onUpdateState -= onUpdateBrainState;

            Events.onFireEvent -= onFireEvent;
            Events.onGlobalEvent -= onGlobalEvent;

            DialogueTree.onCompleteTree -= onCompleteDialogueTree;
        }

        // Update is called once per frame
        void Update()
        {
            if (!load) return;

            EvaluateState();
            UpdateAnimatorFromState(state);

            
            if (Brain.absorption >= 1f) 
            {
                SetAbsorbState(true); // Always set self to NULL
            }


            // Debug window toggle
            if (Input.GetKeyDown(KeyCode.Z)) debug = !debug;
            debugwindow.alpha = (debug) ? 1f : 0f;

			#region Action overrides (DEBUG)
			/*
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
                    var opts = World.Instance.ActiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.NestPop, beacon);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3)) {
                    Actions.Push(ActionType.NestClear);
                }
            }
            if (Input.GetKey(KeyCode.B)){
                if (Input.GetKeyDown(KeyCode.Alpha0)) {
                    var opts = World.Instance.InactiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.BeaconActivate, beacon);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    var opts = World.Instance.InactiveBeacons.PickRandomSubset(1);
                    var beacon = opts.Count() > 0 ? opts.ElementAt(0) : null;

                    Actions.Push(ActionType.BeaconDestroy, beacon);
                }
            }
            if (Input.GetKeyDown(KeyCode.M)) {
                var opts = World.Instance.InactiveBeacons.PickRandomSubset(1);
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
            */
			#endregion
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
            /*
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
            */
        }

        void RespondToPlayerAction(EVENTCODE @event)
        {
            if (@event == EVENTCODE.NESTKICK) 
            {
                if (World.STATE == GAMESTATE.INTRO) {
                    Dialogue.FetchDialogueFromTree(introDialogueTree); // Move to next dialogue node in intro
                    if (Dialogue.inprogress) Dialogue.Advance();
                }
            }

            /*
            return; //IGNORE PLAYER EVENTS FOR NOW


            float base_probability = 1f - Mathf.Pow(Brain.stance, 2f);
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
                        Dialogue.Push("I like to kick nests too, y'know????");
                        Actions.Push(ActionType.NestKick, immediate: true, delay:.2f);
                        break;
                    case EVENTCODE.NESTSPILL:
                        if (Brain.mood > 0f) Actions.Push(ActionType.BeaconActivate, immediate: true);
                        else Actions.Push(ActionType.NestKick, immediate: true);
                        break;
                    default:
                        break;
                }
            }
            */
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

        void TriggerDialogue()
        {
            var mood = Mathf.RoundToInt(Brain.mood);

            Dialogue.autoprogress = false;
            Dialogue.FetchDialogueFromTree(mood);
        }

        public void UpdateCurrentDialogueNode(int nodeID)
        {
            Save.dialogueNode = nodeID;
        }

        public void UpdateDialogueVisited(int[] nodeIDs)
        {
            Save.dialogueVisited = nodeIDs;
        }


        public void React()
        {
            var rand = Random.Range(0f, 1f);
            if (rand > BrainPreset.reactionProbability)
                return;

            EVENTCODE @event = Events.LAST_EVENT;
            Mood state = Brain.moodState;
        }

        public void Comment()
        {
            bool comment = true;

            if (!Dialogue.isAlertActive) 
            {
                float r = Random.Range(0f, 1f);
                comment = (r <= BrainPreset.commentProbabilty);
            }

            if(comment)
                Dialogue.Comment(Brain.moodState, Brain.stanceState);
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

        void NormalDialogueBehaviour()
        {
            Dialogue.autoprogress = true;
        }

        void IntroDialogueBehaviour()
        {
            Dialogue.autoprogress = false;
            Dialogue.FetchDialogueFromTree(introDialogueTree); // Move to next dialogue node in intro
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

        #region Dialogue callbacks

        void onCompleteDialogueTree(DialogueTree tree)
        {
            if (tree == introDialogueTree)
                World.MoveToState(GAMESTATE.GAME); // Move to game state
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
            SetAbsorbState(action.auto);
            if (action.cast) 
                animator.SetTrigger("cast");
        }

        #endregion

        #region Brain callbacks

        void onUpdateBrainState(Mood mood, Stance stance)
        {
            Debug.LogFormat("Wizard became {0} and {1}", mood, stance);
            Dialogue.QueueComment();
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

        #region World callbacks

        public void onUpdateGameState(GAMESTATE state)
        {
            if (state == GAMESTATE.INTRO)
                IntroDialogueBehaviour();
            else
                NormalDialogueBehaviour();
        }

		#endregion

		#region Sun callbacks

		void onDayNightCycle() 
        {
            /*
            return; // IGNORE SUN CYCLE FOR NOW

            float base_probability = Mathf.Pow(Brain.stance, 2f);
            float min = BrainPreset.minimumDayNightActionProbability;
            float max = BrainPreset.maximumDayNightActionProbability;

            float prob = Mathf.Clamp(base_probability, min, max);
            if (Random.Range(0f, 1f) <= prob)
                EnactAction();
            */
        }

        #endregion

        #region Event callbacks

        void onFireEvent(EVENTCODE @event, AGENT a, AGENT b, string detail) 
        {
            if (a == AGENT.Wizard)
                React(); // React to last event

            if (a != AGENT.User) return; // Ignore all non-player events
            RespondToPlayerAction(@event);
        }

        void onGlobalEvent(EVENTCODE @event)
        {
            if (World.STATE == GAMESTATE.INTRO) return;

            TriggerDialogue();
        }

        #endregion

        #region Appearance

        public void SetAbsorbState(bool absorbed)
        {
            if (Brain.absorption >= 1f) {
                Renderer.material = null;
                return;
            }

            override_absorb = absorbed;

            if (absorbed) Renderer.material = null;
            else Renderer.material = DefaultMaterial;
        }

        bool override_absorb = false;

        IEnumerator Absorbing()
        {
            while (true) 
            {
                var absorb = Brain.absorption;

                if (absorb >= 1f || override_absorb)
                    Renderer.material = null;
                else 
                {
                    yield return new WaitForSeconds(.0067f);

                    float threshold = Mathf.Pow(absorb, 3f);
                    if (Random.Range(0f, 1f) < threshold)
                        Renderer.material = null;
                    else
                        Renderer.material = DefaultMaterial;
                }

                yield return null;
            }
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
