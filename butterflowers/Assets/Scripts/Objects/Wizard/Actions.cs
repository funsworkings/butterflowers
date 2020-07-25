﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using System.IO;
using obj = System.Object;
using System.Linq;
using System.Resources;
using UIExt;
using UIExt.Behaviors.Visibility;

namespace Wizard {

    public class Actions : MonoBehaviour {

        #region Events

        public System.Action<Action> onEnact, onComplete;

        #endregion

        #region External

        Library Library;
        World Manager;
        CameraManager CameraManager;
        Nest Nest;

        #endregion

        #region Internal

        public enum Type 
        { 
            None, 

            Dialogue, 
            Emote,
            Gesture,
            Picture,

            BeaconActivate,
            BeaconDestroy,

            NestKick,
            NestPop,
            NestClear,

            MoveTo,
            Inspect
        }

        private string parseTypeName(Type type)
        {
            if (type == Type.None) return null;
            return System.Enum.GetName(typeof(Type), type);
        }

        public enum Emote 
        {
            None,
            Happy,
            Sad,
            Confused,
            Tired
        }

        [System.Serializable]
        public class Action {
            public Type type = Type.None;
            public obj dat = null;
            public bool immediate = false;
            public float delay = 0f;
            public bool auto = false;

            public string debug {
                get
                {
                    if (dat == null) return "NULL";

                    if (type == Type.Emote) {
                        return ((Emote)dat).ToString();
                    }
                    if (type == Type.Gesture) {
                        return ((Gesture)dat).clip.name;
                    }

                    return "???";
                }
            }

            /* * * * * */

            public bool wait => (cast || type == Type.Picture || type == Type.Gesture || type == Type.MoveTo || type == Type.Inspect || type == Type.Dialogue);

            public bool cast {
                get
                {
                    var name = System.Enum.GetName(typeof(Type), type);
                    return name.StartsWith("Beacon") || name.StartsWith("Nest");
                }
            }
            public bool passive => (type != Type.Gesture && type != Type.Picture);
        }

        [System.Serializable]
        public class ActionSequence 
        {
            public Action[] actions;
            public bool immediate = false;
        }

        #endregion

        #region Properties

        [SerializeField] BrainPreset Preset;

        Controller controller;
        Wand wand;
        Brain brain;
        Dialogue dialogue;
        Memories memories;
        Navigation navigation;
        Animator animator;

        [SerializeField] Snapshot snapshot;

        
        [SerializeField] Transform spells_root;
        [SerializeField] GameObject pr_spell;
        [SerializeField] ParticleSystem spells_ps;

        #endregion

        #region Collections

        [SerializeField] List<Action> m_queue = new List<Action>();

        #endregion

        #region Attributes

        [SerializeField] bool m_inprogress = false;
        Action m_currentAction = null;

        // Tracking emote
        Emote currentEmote;

        // Tracking gesture
        Gesture currentGesture;

        // Tracking camera
        Camera previousMainCamera = null;
        [SerializeField] Transform cameraFocalPoint = null;
        bool lookingAtCamera = false;

        // Tracking movement
        Vector3 currentWaypoint = Vector3.zero;

        Transform spell_target = null;

        string currentBody = null;

        #endregion

        #region Accessors

        public Action currentAction => m_currentAction;
        public Action[] queue => m_queue.ToArray();
        public Action nextAction {
            get
            {
                if (available) 
                {
                    if (controller.isFocused) {
                        var passive_actions = queue.Where(action => action.passive);
                        if (passive_actions.Count() > 0)
                            return passive_actions.ElementAt(0);
                    }
                    else
                        return queue[0];
                }

                return null;
            }
        }

        public bool inprogress {
            get
            {
                return m_inprogress;
            }
            set
            {
                m_inprogress = value;
            }
        }

        public bool available => (m_queue != null && m_queue.Count > 0);

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            controller = GetComponent<Controller>();
        }

        void Start()
        {
            brain = controller.Brain;
            memories = controller.Memories;
            animator = controller.Animator;

            Manager = World.Instance;
            Library = Library.Instance;
            Nest = Nest.Instance;
            CameraManager = FindObjectOfType<CameraManager>();
        }

        void OnEnable()
        {
            snapshot.onSuccess += onReceivePicture;

            wand = controller.Wand;
            wand.onGestureEnd += onCompleteGesture;

            navigation = GetComponent<Navigation>();
            //navigation.onMoveToPoint += onMoveToLocation;
            //navigation.onLookAtPoint += onLookAtLocation;

            dialogue = controller.Dialogue;
            dialogue.onCompleteBody += onCompleteDialogue;
            dialogue.onDispose += onDisposeDialogue;

            Spell.Arrived += onSpellArrived;

            StartCoroutine("Refresh");
        }

        void OnDisable()
        {
            snapshot.onSuccess -= onReceivePicture;
            wand.onGestureEnd -= onCompleteGesture;

            wand.onGestureEnd -= onCompleteGesture;
            //navigation.onMoveToPoint -= onMoveToLocation;
           // navigation.onLookAtPoint -= onLookAtLocation;

            dialogue.onCompleteBody -= onCompleteDialogue;
            dialogue.onDispose -= onDisposeDialogue;

            Spell.Arrived -= onSpellArrived;

            StopCoroutine("Refresh");
        }

        #endregion

        #region Internal

        IEnumerator Refresh()
        {
            while (true) {

                if (!inprogress) {
                    m_currentAction = null;

                    var action = nextAction;

                    if (available && action != null) 
                    {
                        var next = queue[0];
                        float delay = next.delay;

                        yield return new WaitForSeconds(delay);

                        Pop(action);
                    }

                }

                yield return null;
            }
        }

        #endregion

        #region Operations

        public void Push(Action action)
        {
            Push(action.type, action.dat, action.immediate);
        }

        public void Push(ActionSequence actions)
        {
            var seq = actions.actions;
            var immediate = actions.immediate;

            for (int i = 0; i < seq.Length; i++) {
                var action = seq[i];
                action.immediate = false;

                if (immediate)
                    m_queue.Insert(i, action);
                else
                    m_queue.Add(action);
            }
        }

		public void Push(Type type, obj dat = null, bool immediate = false, float delay = -1f, bool auto = false)
        {
            Action action = new Action();
            action.type = type;
            action.dat = dat;
            action.immediate = immediate;
            action.delay = (delay < 0f)? Random.Range(Preset.minimumTimeBetweenActions, Preset.maximumTimeBetweenActions) : delay;
            action.auto = auto;

            if (immediate) 
            {
                if (available) 
                    m_queue.Insert(0, action);
                else
                    m_queue.Add(action);

                if (currentAction != null && inprogress) 
                {
                    bool cancel = CancelAction(currentAction);
                    inprogress = !cancel;
                }

                if(action.delay == 0f) Pop();
            }
            else
                m_queue.Add(action);
        }

        void Pop(Action action = null)
        {
            if (!available) return;
            if (inprogress) return;

            int index = 0;
            if (action != null) {
                index = queue.ToList().IndexOf(action);

                var _action = m_currentAction = m_queue[index];
                if (!_action.wait)
                    ParseAction(_action);
                else {
                    inprogress = true;

                    if (_action.type == Type.Picture)
                        TakePicture();
                    else if (_action.type == Type.Gesture) {
                        currentGesture = (Gesture)_action.dat;
                        DoGesture(currentGesture);
                    }
                    else if (_action.type == Type.MoveTo) {
                        currentWaypoint = (Vector3)_action.dat;

                        animator.SetTrigger("reset_to_idle");

                        navigation.MoveTo(currentWaypoint);
                        navigation.LookAt(null);
                    }
                    else if (action.type == Type.Inspect) {
                        animator.SetTrigger("inspect");

                        var target = (Transform)_action.dat;
                        navigation.LookAt(target);
                    }
                    else if (action.type == Type.Dialogue) {
                        string body = (string)_action.dat;

                        if (string.IsNullOrEmpty(body)) {
                            var mood = brain.mood;
                            currentBody = dialogue.FetchDialogueFromTree(Mathf.RoundToInt(mood));
                        }
                        else {
                            currentBody = body;
                            dialogue.Push(body);
                        }
                    }
                }

                if (_action.type != Type.None) 
                {
                    if (onEnact != null)
                        onEnact(_action);
                }

            }

            m_queue.RemoveAt(index); // Remove el from queue
        }

        public void Dispose()
        {
            m_queue.Clear();

            if (inprogress) 
            {
                bool clear = CancelAction(currentAction);
                if (clear) {
                    m_currentAction = null;
                    inprogress = false;
                }
            }
        }

        #endregion

        #region Internal

        void ParseAction(Action action)
        {
            var dat = action.dat;
            var type = action.type;
            var t = System.Enum.GetName(typeof(Type), type);

            if (t.StartsWith("Beacon")) 
            {
                Beacon beacon = (Beacon)dat;

                if (beacon != null) {

                    if (type == Type.BeaconActivate)
                        wand.ActivateBeacon(beacon);
                    else if (type == Type.BeaconDestroy)
                        wand.DestroyBeacon(beacon);
                }
                
            }
            else if (t.StartsWith("Nest")) 
            {
                if (type == Type.NestPop) 
                {
                    Beacon beacon = (Beacon)dat;

                    if (beacon == null)
                        wand.PopLastBeaconFromNest();
                    else
                        wand.PopBeaconFromNest(beacon);
                }
                else 
                {
                    if (type == Type.NestKick) {
                        Wand.Kick kick = (Wand.Kick)dat;
                        wand.KickNest(kick);
                    }
                    else if (type == Type.NestClear)
                        wand.ClearNest();
                }
            }
            else // Miscellaneous
            {
                if (action.type == Type.Emote) 
                {
                    Emote[] emotes = brain.GetPossibleEmotes();

                    if (emotes.Length > 0) {
                        Emote emote = emotes.PickRandomSubset(1)[0];
                        currentEmote = emote;

                        HandleEmote(emote);
                    }
                }
            }

            if (!action.wait) inprogress = false;
        }

        bool CancelAction(Action action)
        {
            bool success = false;

            if (action.type == Type.MoveTo) {
                navigation.Stop();
                success = true;
            }
            else if (action.type == Type.Emote || action.type == Type.Inspect) {
                animator.SetTrigger("reset_to_idle");
                success = true;
            }
            else if (action.type == Type.Gesture) {
                bool flag = wand.CancelGesture();
                success = flag;
            }

            return success;
        }

        #endregion

		#region Camera

		void TakePicture()
        {
            var camera = snapshot.camera;

            previousMainCamera = CameraManager.MainCamera;
            CameraManager.MainCamera = camera;
            camera.enabled = true;

            //lookingAtCamera = false;
            //currentWaypoint = cameraFocalPoint.position;
            //navigation.LookAt(cameraFocalPoint);

            StartCoroutine("TakingPicture");
        }

        IEnumerator TakingPicture()
        {
            //while (!lookingAtCamera) 
              //  yield return null;

            yield return new WaitForSeconds(.167f);
            snapshot.Capture();
        }

        void onReceivePicture(Texture2D image) 
        {
            return;

            var name = Extensions.RandomString(12);
            var camera = snapshot.camera;

            Library.SaveTexture(name, image);

            CameraManager.MainCamera = previousMainCamera;

            camera.enabled = false;
            previousMainCamera = null;

            Events.ReceiveEvent(EVENTCODE.PHOTOGRAPH, AGENT.Wizard, AGENT.World, details: name+".jpg");
            if(currentAction.type == Type.Picture && inprogress) 
                inprogress = false;
        }

        #endregion

        #region Animation callbacks

        public void onCastSpell()
        {
            if (inprogress) 
            {
                if (currentAction.cast)
                {
                    var t = System.Enum.GetName(typeof(Type), currentAction.type);

                    if (t.StartsWith("Beacon")) 
                    {
                        Beacon b = (Beacon)currentAction.dat;

                        if (b != null) 
                        {
                            CastSpell(b.transform);
                            return;
                        }
                    }
                    else if (t.StartsWith("Nest")) 
                    {
                        CastSpell(Nest.transform, true);
                        return;
                    }

                    inprogress = false; // Force quit action
                }
            }
        }

        void onCompleteGesture(Gesture gesture)
        {
            if (currentGesture.clip != gesture.clip) return;

            if (inprogress) {
                if (currentAction.type == Type.Gesture) {
                    inprogress = false;
                }
            }
        }

        public void onInspect()
        {
            if (inprogress) {
                if (currentAction.type == Type.Inspect) {
                    inprogress = false;

                    navigation.ResetLookAtToDefault();
                }
            }
        }

        #endregion

        #region Emotes

        void HandleEmote(Emote emote)
        {
            var animator = controller.Animator;

            switch (emote) {
                case Emote.Happy:
                    animator.SetTrigger("happy");
                    break;
                case Emote.Sad:
                    animator.SetTrigger("sad");
                    break;
                case Emote.Tired:
                case Emote.Confused:
                case Emote.None:
                default:
                    break;
            }
        }

        #endregion

        #region Gestures

        void DoGesture(Gesture gesture)
        {
            bool success = wand.EnactGesture(gesture);
        }

        #endregion

        #region Dialogue

        void onCompleteDialogue(string body)
        {
            if (inprogress) {
                if (currentAction.type == Type.Dialogue && body == currentBody) 
                {
                    inprogress = false;
                }
            }
        }

        void onDisposeDialogue()
        {
            if (inprogress && currentAction.type == Type.Dialogue)
                inprogress = false;
        }

        #endregion

        #region Spells

        void CastSpell(Transform target, bool track = false)
        {
            Vector3 waypoint = target.position;

            GameObject spell_instance = Instantiate(pr_spell, spells_root.position, pr_spell.transform.rotation, spells_root);
            Spell spell = spell_instance.GetComponent<Spell>();

            spell.MoveTo(target, track);
            spell_target = target;
        }

        void onSpellArrived(Transform target, bool success)
        {
            if (inprogress && currentAction.cast && target == spell_target) 
            {
                if (success) // target object was there
                    ParseAction(currentAction);

                spell_target = null;
                inprogress = false;
            }
        }

		#endregion

		#region Navigation callbacks

		void onMoveToLocation(Vector3 location)
        {
            if (location != currentWaypoint) return;

            if (currentAction.type == Type.MoveTo && inprogress) {
                inprogress = false;
                navigation.ResetLookAtToDefault();
            }
        }

        void onLookAtLocation(Vector3 location)
        {
            if (location != currentWaypoint) return;

            if (currentAction.type == Type.Picture) 
                lookingAtCamera = true;
        }

		#endregion

		#region Deprecated

		void CreateRandomBeacon()
        {
            Memory memory = memories.FetchRandomItem();
            CreateBeaconFromMemory(memory);
        }

        void CreateBeaconFromMemory(Memory mem)
        {
            if (mem == null) return;

            Texture2D tex = mem.image;
            Beacon beacon = World.Instance.CreateBeaconForWizard(tex);

            if (beacon != null) {
                beacon.fileEntry = null;
            }
        }

        #endregion

    }

}
