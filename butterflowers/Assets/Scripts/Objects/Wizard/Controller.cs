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
                if (Input.GetKeyDown(KeyCode.W)) Dialogue.Push("I am truly a wizard y'know s'good <sprite name=\"EmojiOne_0\"> it is my nature");
                if (Input.GetKeyDown(KeyCode.RightArrow)) Dialogue.Advance();
            }
            else {
                Dialogue.Dispose();
            }

            EvaluateState();
            UpdateAnimatorFromState(state);
        }

        #endregion

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

        #region Navigation

        public void MoveTo(Vector3 location)
        {
            navAgent.SetDestination(location);
        }

        #endregion

    }

}
