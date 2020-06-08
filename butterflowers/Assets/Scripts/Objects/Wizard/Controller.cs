using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;
using UnityEngine.AI;

namespace Wizard {

    public class Controller: MonoBehaviour {
        [SerializeField] Wand wand;

        IK ik;

        #region External

        GameDataSaveSystem Save;

		#endregion

		#region Internal

		public enum State { Idle, Walk, Spell, Rest }

        #endregion

        #region Properties

        [SerializeField] WorldPreset Preset;

        Animator animator;
        NavMeshAgent navAgent;

        FocalPoint Focus;
        Brain Brain;
        Dialogue Dialogue;
        Audio Audio;

        [SerializeField] Memories m_memories;

        #endregion

        #region Attributes

        [SerializeField] State state = State.Idle;
        [SerializeField] Texture2D[] memories = new Texture2D[] { };
        [SerializeField] int dialogueRoute = 0;

        bool load = false;

		#endregion

		#region Accessors

        public bool isFocused {
            get
            {
                if (Focus == null) return false;
                return Focus.focus;
            }
        }

        public Memories Memories {
            get
            {
                return m_memories;
            }
        }

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            ik = GetComponentInChildren<IK>();
            animator = GetComponentInChildren<Animator>();
            navAgent = GetComponent<NavMeshAgent>();

            Focus = GetComponent<FocalPoint>();
            Brain = GetComponent<Brain>();
            Dialogue = GetComponent<Dialogue>();
            Audio = GetComponent<Audio>();
        }

        IEnumerator Start()
        {
            Save = GameDataSaveSystem.Instance;
            while (Save == null || !Save.load) yield return null;

            Dialogue.nodeID = Save.dialogueNode;

            load = true;
        }

        void OnEnable()
        {
            Focus.onFocus += PushDialogue;
            Focus.onLoseFocus += ClearDialogue;
        }

        void OnDisable()
        {
            Focus.onFocus -= PushDialogue;
            Focus.onLoseFocus -= ClearDialogue;
        }

        // Update is called once per frame
        void Update()
        {
            if (!load) return;

            //UpdateLookAt();

            if (Focus.focus) {
                if (Input.GetKeyDown(KeyCode.RightArrow)) Dialogue.Advance();
            }
            if (Input.GetKeyDown(KeyCode.X)) Dialogue.FetchDialogueFromTree(dialogueRoute);

            if (Input.GetKeyDown(KeyCode.S)) ActivateRandomBeacon();
            if (Input.GetKeyDown(KeyCode.T)) CreateRandomBeacon();

            EvaluateState();
            UpdateAnimatorFromState(state);

            if (Input.GetKeyDown(KeyCode.Semicolon))
                Brain.AddMemory(memories[0]);
        }

		#endregion

		#region Internal

		void EvaluateState()
        {
            float speed = navAgent.velocity.magnitude;

            //Debug.LogFormat("Nav Agent --- speed = {0}", speed);
            if (speed > .1f) {  // Set to moving state
                state = State.Walk;
            }
            else {  // Set to idle state
                if(state != State.Rest)
                    state = State.Idle;
            }
        }

		#endregion

		#region Animation

		public void UpdateAnimatorFromState(State state)
        {
            animator.SetBool("walking", state == State.Walk);
            animator.SetBool("resting", state == State.Rest);

            switch (state) {
                case State.Walk:
                    break;
                case State.Idle:
                    break;
                case State.Rest:
                    break;
                case State.Spell:
                    break;
                default:
                    break;

            }
        }

        void UpdateLookAt()
        {
            if (ik == null)
                return;

            ik.lookAtWeight = (wand.spells) ? 1f : 0f;
            ik.lookAtPosition = (wand.worldPosition);
        }

		#endregion

		#region Navigation

		public void MoveTo(Vector3 location)
        {
            navAgent.SetDestination(location);
        }

        #endregion

        #region Spells

        public void ActivateRandomBeacon()
        {
            var beacon = Manager.Instance.FetchRandomBeacon();
            bool spell = (beacon != null);
            if (spell) {
                beacon.Discover();
                animator.SetTrigger("spell");
            }
        }

        public void CreateRandomBeacon()
        {
            if (memories.Length == 0) return;

            Memory memory = Memories.FetchRandomItem();
            CreateBeaconFromMemory(memory);
        }

        public void CreateBeaconFromMemory(Memory mem)
        {
            if (mem == null) return;

            Texture2D tex = mem.image;
            Beacon beacon = Manager.Instance.CreateBeaconForWizard(tex);

            if (beacon != null) {
                beacon.fileEntry = null;
                onCastSpell();
            }
        }

        void onCastSpell() {
            animator.SetTrigger("spell");
        }

        #endregion

        #region Miscellaneous

        public void WakeUp()
        {
            state = State.Idle;
        }

        #endregion

        #region Dialogue

        void PushDialogue()
        {
            Dialogue.PushAllFromQueue();
        }

        void ClearDialogue()
        {
            Dialogue.Dispose();
        }

        public void UpdateCurrentDialogueNode(int nodeID)
        {
            if (!Preset.reset)
                Save.dialogueNode = nodeID;
            else
                Save.dialogueNode = -1;
        }

		#endregion

	}

}
