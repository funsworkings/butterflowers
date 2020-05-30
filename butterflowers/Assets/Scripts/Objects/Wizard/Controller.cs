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

        #endregion

        #region Attributes

        State state = State.Idle;

        [SerializeField] string message = "";

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
            }
            else {
                Dialogue.Dispose();
            }

            if (Input.GetKeyDown(KeyCode.S)) ActivateRandomBeacon();

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
                state = State.Idle;
            }
        }

		#endregion

		#region Animation

		public void UpdateAnimatorFromState(State state)
        {
            switch (state) {
                case State.Walk:
                    animator.SetBool("walking", true);
                    break;
                case State.Idle:
                case State.Rest:
                case State.Spell:
                default:
                    animator.SetBool("walking", false);
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
            bool spell = Manager.Instance.ActivateRandomBeacon();
            if(spell) animator.SetTrigger("spell");
        }

		#endregion

	}

}
