using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Wizard {

    public class Controller: MonoBehaviour {
        Wand wand;

        IK ik;

        #region Internal

        public enum State { Idle, Walk, Spell, Rest }

        #endregion

        #region Properties

        Animator animator;
        NavMeshAgent navAgent;

        FocalPoint Focus;
        Dialogue Dialogue;
        Audio Audio;

        public MemoryBank Memories;

        #endregion

        #region Attributes

        [SerializeField] State state = State.Idle;

        [SerializeField] [TextArea(1, 10)] string message = "fuck off mannnn  :i:EmojiOne_0:i:  that's what im tryna say OK? :i:EmojiOne_0:i:";
        [SerializeField] Texture2D[] memories = new Texture2D[] { };

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            ik = GetComponentInChildren<IK>();
            animator = GetComponentInChildren<Animator>();
            navAgent = GetComponent<NavMeshAgent>();

            Focus = GetComponent<FocalPoint>();
            Dialogue = GetComponent<Dialogue>();
            Audio = GetComponent<Audio>();
        }

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            wand = FindObjectOfType<Wand>();
        }

        // Update is called once per frame
        void Update()
        {
            //UpdateLookAt();

            if (Focus.focus) {
                if (Input.GetKeyDown(KeyCode.W)) Dialogue.Push(message);
                if (Input.GetKeyDown(KeyCode.RightArrow)) Dialogue.Advance();
                if (Input.GetKeyDown(KeyCode.X)) Dialogue.FetchDialogueFromTree();
            }
            else {
                Dialogue.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.S)) ActivateRandomBeacon();
            if (Input.GetKeyDown(KeyCode.T)) CreateRandomBeacon();

            EvaluateState();
            UpdateAnimatorFromState(state);
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

	}

}
