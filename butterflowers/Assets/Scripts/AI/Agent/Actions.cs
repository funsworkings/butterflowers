using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using System.IO;
using obj = System.Object;
using System.Linq;
using System.Resources;
using AI.Types;
using uwu.UI;
using uwu.Camera;
using uwu.Extensions;
using uwu.Snippets;

namespace AI.Agent
{

    public class Actions : MonoBehaviour
    {

        // Events

        public System.Action<Action> onEnact, onComplete;

        // External

        CameraManager CameraManager;
        [SerializeField] Nest Nest;

        // Properties

        [SerializeField] BrainPreset Preset;
        [SerializeField] Wand wand;
        [SerializeField] GameObject avatar;
                         Mesh mesh;

        Brain Brain;

        // Collections

        [SerializeField] List<Action> m_queue = new List<Action>();

        // Attributes

        [SerializeField] bool m_inprogress = false;
        Action m_currentAction = null;

        #region Accessors

        public Action currentAction => m_currentAction;
        public Action[] queue => m_queue.ToArray();

        public Action nextAction
        {
            get
            {
                if (available) {
                    return queue[0];
                }

                return null;
            }
        }

        public bool inprogress
        {
            get { return m_inprogress; }
            set { m_inprogress = value; }
        }

        public bool available => (m_queue != null && m_queue.Count > 0);

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            Brain = GetComponent<Brain>();

            avatar.SetActive(false);
            mesh = avatar.GetComponentInChildren<Mesh>(true);
        }

        void Start()
        {
            CameraManager = FindObjectOfType<CameraManager>();
        }

        void Update()
        {
            avatar.SetActive(Brain.Self);
        }

        void OnEnable()
        {
            mesh.DidTeleport += OnDidTeleport;
            mesh.DidCastSpell += OnDidCastSpell;
            
            StartCoroutine("Refresh");
        }

        void OnDisable()
        {
            mesh.DidTeleport -= OnDidTeleport;
            mesh.DidCastSpell -= OnDidCastSpell;
            
            StopCoroutine("Refresh");
        }

        #endregion


        IEnumerator Refresh()
        {
            while (true) {

                if (!inprogress) {
                    m_currentAction = null;

                    var action = nextAction;

                    if (available && action != null) {
                        var next = queue[0];
                        float delay = next.delay;

                        yield return new WaitForSeconds(delay);

                        if(Brain.isActive) // Only pop action from queue if active
                            Pop(action);
                    }

                }

                yield return null;
            }
        }


        #region Action queue

        public void Push(Action action)
        {
            Push(action.@event, action.dat, action.advertiser, action.immediate);
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

        public void Push(EVENTCODE @event, object dat = null, Advertiser advertiser = null, bool immediate = false, float delay = -1f, bool auto = false)
        {
            Action action = new Action();
            action.@event = @event;
            action.dat = dat;
            action.advertiser = advertiser;
            action.immediate = immediate;
            action.delay = (delay < 0f)
                ? Random.Range(Preset.minimumTimeBetweenActions, Preset.maximumTimeBetweenActions)
                : delay;
            action.auto = auto;

            if (immediate) {
                if (available)
                    m_queue.Insert(0, action);
                else
                    m_queue.Add(action);

                if (currentAction != null && inprogress) {
                    bool cancel = CancelAction(currentAction);
                    inprogress = !cancel;
                }

                if (action.delay == 0f) Pop();
            }
            else
                m_queue.Add(action);
        }

        void Pop(Action action = null)
        {
            if (!available) return;
            if (Brain.Self && mesh.inprogress) return;

            if (action != null) {
                if (action != currentAction) 
                {
                    m_currentAction = action;

                    if (Brain.Self) 
                    {
                        if (!inprogress) {
                            var advertiser = currentAction.advertiser;
                            if (advertiser != null)
                                mesh.SpellAtLocation(advertiser.transform.position);
                            else
                                mesh.Spell();

                            inprogress = true;
                        }
                    }
                    else {
                        Pop(currentAction);
                    }
                }
                else {
                    ParseAction(action);
                    m_queue.RemoveAt(0); // Remove el from queue
                }
            }
        }

        public void Dispose()
        {
            m_queue.Clear();

            if (inprogress) {
                bool clear = CancelAction(currentAction);
                if (clear) {
                    m_currentAction = null;
                    inprogress = false;
                }
            }
        }

        #endregion

        #region Action parse + cancel

        void ParseAction(Action action)
        {
            var dat = action.dat;
            var @event = action.@event;
            var t = System.Enum.GetName(typeof(EVENTCODE), @event);
            var advertiser = action.advertiser;

            if (t.StartsWith("BEACON")) {
                Beacon beacon = (Beacon) dat;

                if (beacon != null) {

                    if (@event == EVENTCODE.BEACONACTIVATE)
                        wand.ActivateBeacon(beacon);
                    else if (@event == EVENTCODE.BEACONDELETE)
                        wand.DestroyBeacon(beacon);
                    else if(@event == EVENTCODE.BEACONPLANT)
                        wand.PlantBeacon(beacon);
                    
                }
            }
            else if (t.StartsWith("NEST")) 
            {
                if (@event == EVENTCODE.NESTPOP) {
                    Beacon beacon = (Beacon) dat;

                    if (beacon == null)
                        wand.PopLastBeaconFromNest();
                    else
                        wand.PopBeaconFromNest(beacon);
                }
                else if (@event == EVENTCODE.NESTKICK) {
                    Wand.Kick kick = new Wand.Kick();
                    if (dat == null)
                        kick.useDirection = false;
                    
                    wand.KickNest(kick);
                }
                else if (@event == EVENTCODE.NESTCLEAR)
                {
                    wand.ClearNest();
                }
            }
            else // Miscellaneous events
            {
                if (@event == EVENTCODE.REFOCUS) {
                    Focusable focus = (Focusable) dat;
                    wand.Refocus(focus);
                }
            }
            
            if (advertiser != null) 
                advertiser.CaptureAllRewards(@event, true); // Retrieve all rewards for advertisement
            

            action.Complete();
            inprogress = false;
        }

        bool CancelAction(Action action)
        {
            action.Cancel();
            return true;
        }
        
        #endregion
        
        #region Mesh animation callbacks

        void OnDidCastSpell()
        {
            Pop(currentAction);
        }

        void OnDidTeleport()
        {
            
        }
        
        #endregion

    }

}
