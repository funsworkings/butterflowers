using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using System.IO;
using obj = System.Object;
using System.Linq;
using System.Resources;
using Gesture = Wand.Gesture;
using UIExt;
using UIExt.Behaviors.Visibility;

namespace Wizard {

    public class Actions: MonoBehaviour {

        #region Events

        public System.Action<Action> onEnact, onDispose;

        #endregion

        #region External

        Library Library;
        Manager Manager;
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
            NestClear
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

            public bool wait = false;
            public bool immediate = false;

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

            public bool cast {
                get
                {
                    var name = System.Enum.GetName(typeof(Type), type);
                    return name.StartsWith("Beacon") || name.StartsWith("Nest");
                }
            }
            public bool passive => (type != Type.Gesture && type != Type.Picture);
        }

        #endregion

        #region Properties

        [SerializeField] BrainPreset Preset;

        Controller controller;
        Wand wand;
        Brain brain;
        Dialogue dialogue;
        Memories memories;

        [SerializeField] Snapshot snapshot;
        [SerializeField] ParticleSystem spells_ps;

        #endregion

        #region Collections

        [SerializeField] List<Action> m_queue = new List<Action>();

        #endregion

        #region Attributes

        [SerializeField] bool m_inprogress = false;
        [SerializeField] Action m_currentAction = null;

        Camera previousMainCamera = null;

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
            dialogue = controller.Dialogue;
            memories = controller.Memories;
            wand = controller.Wand;

            Manager = Manager.Instance;
            Library = Library.Instance;
            Nest = Nest.Instance;
            CameraManager = FindObjectOfType<CameraManager>();
        }

        void OnEnable()
        {
            snapshot.onSuccess += onReceivePicture;

            wand = controller.Wand;
            wand.onGestureEnd += onCompleteGesture;

            StartCoroutine("Refresh");
        }

        void OnDisable()
        {
            snapshot.onSuccess -= onReceivePicture;
            wand.onGestureEnd -= onCompleteGesture;

            StopCoroutine("Refresh");
        }

        #endregion

        #region Internal

        IEnumerator Refresh()
        {
            while (true) {

                if (!inprogress) {

                    var action = nextAction;

                    if (available && action != null) 
                    {
                        if (!nextAction.immediate) 
                        {
                            float delay = Random.Range(Preset.minimumTimeBetweenActions, Preset.maximumTimeBetweenActions);
                            yield return new WaitForSeconds(delay);
                        }

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

		public void Push(Type type, obj dat = null, bool immediate = false)
        {
            Action action = new Action();
            action.type = type;
            action.dat = dat;
            action.immediate = immediate;
            action.wait = (action.cast || type == Type.Picture || type == Type.Gesture);

            if (immediate) 
            {
                if (available)
                    m_queue.Insert(0, action);
                else
                    m_queue.Add(action);

                Pop();
            }
            else
                m_queue.Add(action);
        }

        void Pop(Action action = null)
        {
            if (!available) return;
            if (inprogress) return;

            int index = 0;
            if (action != null)
                index = queue.ToList().IndexOf(action);

            var _action = m_currentAction = m_queue[index];
            if (!_action.wait)
                ParseAction(_action);
            else {
                inprogress = true;
                if (_action.type == Type.Picture)
                    TakePicture();
                else if (_action.type == Type.Gesture)
                    DoGesture((Gesture)_action.dat);
            }

            if (_action.type != Type.None) {
                if (onEnact != null)
                    onEnact(_action);
            }

            m_queue.RemoveAt(index); // Remove el from queue
        }

        public void Dispose()
        {
            m_queue.Clear();
            m_currentAction = null;
            inprogress = false;
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

                if (type == Type.BeaconActivate)
                    wand.ActivateBeacon(beacon);
                else if (type == Type.BeaconDestroy)
                    wand.DestroyBeacon(beacon);
                
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
                    if (type == Type.NestKick)
                        wand.KickNest();
                    else if (type == Type.NestClear)
                        wand.ClearNest();
                }
            }
            else // Miscellaneous
            {
                if (action.type == Type.Dialogue) 
                {
                    string body = (string)dat;

                    if (string.IsNullOrEmpty(body)) {
                        var mood = brain.mood;
                        dialogue.FetchDialogueFromTree(Mathf.RoundToInt(mood));
                    }
                    else {
                        dialogue.Push(body);
                    }
                }
                else if (action.type == Type.Picture) 
                {
                    TakePicture();
                }
                else if (action.type == Type.Emote) 
                {
                    Emote[] emotes = brain.GetPossibleEmotes();

                    if (emotes.Length > 0) {
                        Emote emote = emotes.PickRandomSubset(1)[0];
                        HandleEmote(emote);
                    }
                }
                else if (action.type == Type.Gesture) 
                {
                    Gesture gesture = (Gesture)dat;
                    DoGesture(gesture);
                }
            }

            if(!action.wait) inprogress = false;
        }

        #endregion

		#region Camera

		void TakePicture()
        {
            var camera = snapshot.camera;

            previousMainCamera = CameraManager.MainCamera;
            CameraManager.MainCamera = camera;
            camera.enabled = true;

            StartCoroutine("TakingPicture");
        }

        IEnumerator TakingPicture()
        {
            var ik = controller.GetComponentInChildren<IK>();
            while (ik.lookAtWeight < .87f) yield return null;

            snapshot.Capture();
        }

        void onReceivePicture(Texture2D image) 
        {
            var name = Extensions.RandomString(12);
            var camera = snapshot.camera;

            Library.SaveTexture(name, image);

            CameraManager.MainCamera = previousMainCamera;

            camera.enabled = false;
            previousMainCamera = null;

            if(currentAction.type == Type.Picture && inprogress) 
                inprogress = false;
        }

        #endregion

        #region Wand callbacks

        public void onCastSpell()
        {
            if (inprogress) 
            {
                if (currentAction.cast)
                {
                    ParseAction(currentAction);
                    inprogress = false;

                    spells_ps.Play();
                }
            }
        }

        void onCompleteGesture(Gesture gesture)
        {
            if (inprogress) {
                if (currentAction.type == Type.Gesture) {
                    inprogress = false;
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

        void DoRandomGesture()
        {
            wand.DoRandomGesture();
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
            Beacon beacon = Manager.Instance.CreateBeaconForWizard(tex);

            if (beacon != null) {
                beacon.fileEntry = null;
            }
        }

        #endregion

    }

}
