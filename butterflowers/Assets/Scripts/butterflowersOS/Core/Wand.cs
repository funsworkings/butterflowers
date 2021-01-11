using System.Collections;
using System.Collections.Generic;
using System.Linq;
using B83.Win32;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Presets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using uwu.Gameplay;
using uwu.Snippets;
using uwu.UI.Behaviors.Visibility;
using uwu.UI.Extras;
using Cursor = uwu.Snippets.Cursor;

namespace butterflowersOS.Core
{
    public class Wand : Interacter<Entity>, IReactToSunCycle
    {
        #region Internal

        public enum Interaction
        {
            Normal,
            Drag
        }

        public enum DragContext
        {
            Release,
            
            Addition,
            Flower,
            Plant,
            Destroy
        }
    
        #endregion
    
        // External

        Sun sun;
    
        [SerializeField] WorldPreset preset;
        [SerializeField] ButterflowerManager butterflowers;
        [SerializeField] BeaconManager beacons;
        [SerializeField] Nest nest;

        // Properties
    
        SimpleVelocity _simpleVelocity;
        
        // Attributes

        [Header("General")]
        [SerializeField] bool m_spells = true;

        [SerializeField] Cursor cursor;
        [SerializeField] CustomCursor cursor_icon;
        [SerializeField] float cursorToWorldRate = 1f;

        public bool infocus = true;

        [Header("Interaction")] 
        
        [SerializeField] Interaction _interaction = Interaction.Normal;
        
        [SerializeField] LayerMask dragInteractionMask;
        
        IInteractable _interactable = null;
        RaycastHit _interactableHit;
        Collider _interactableCollider = null;

        [SerializeField, FormerlySerializedAs("brushRadius")] float _radius = 12f;

        [Header("Drag")]
            [SerializeField] Beacon beacon = null;
            [SerializeField] DragContext context = DragContext.Release;
            [SerializeField] float defaultPointDistance = 10f;
            [SerializeField] float pointDistance;

        [Header("Layers")] 
            [SerializeField] LayerMask additionMask;
            [SerializeField] LayerMask flowerMask;
            [SerializeField] LayerMask plantMask;
            [SerializeField] LayerMask destroyMask;

        [Header("UI")] 
            [SerializeField] Tooltip info;
            [SerializeField] ToggleOpacity infoOpacity;
            [SerializeField] TMP_Text infoText;

        #region Accessors

        public Vector3 position3d {
            get{
                return camera.ScreenToWorldPoint(new Vector3(cursor.position.x, cursor.position.y, 1f));
            }
        } 

        public Vector3 position2d
        {
            get { return cursor.position; }
        }

        Vector3 m_trajectory2d;
        public Vector3 trajectory2d
        {
            get
            {
                return m_trajectory2d;
            }
        }

        public Vector3 worldPosition {
            get
            {
                if (camera == null)
                    return Vector3.zero;

                return camera.ScreenToWorldPoint(new Vector3(position3d.x, position3d.y, 10f));
            }
        }

        public bool spells
        {
            get {
                return m_spells;
            }
            set
            {
                m_spells = value;
            }
        }

        [SerializeField] Vector3 m_velocity3d;
        public Vector3 velocity3d {
            get
            {
                var cursor = velocity2d * cursorToWorldRate;
            
                var cursorVector = camera.transform.TransformVector(cursor);
                var worldVector = _simpleVelocity.velocity;

                m_velocity3d = cursorVector + worldVector;
                return m_velocity3d;
            }
        }

        [SerializeField] Vector3 m_velocity2d;
        public Vector3 velocity2d
        {
            get
            {
                m_velocity2d = cursor.velocity;
                return m_velocity2d;
            }
        }

        [SerializeField] float m_speed = 0f;
        public float speed 
        {
            get
            {
                m_speed = velocity3d.magnitude;
                return m_speed;
            }
        }

        public float speed2d => velocity2d.magnitude;

        public float radius => _radius;
    
        public Camera Camera => camera;

        AGENT Agent => (AGENT.User);
        World World => World.Instance;

        #endregion

        #region Internal

        [System.Serializable]
        public struct Kick 
        {
            public bool useDirection;
        
            public Vector3 direction;
            public float force;
        }

        #endregion

        #region Monobehaviour callbacks

        void Awake()
        {
            _simpleVelocity = GetComponent<SimpleVelocity>();
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            
            sun = Sun.Instance;
        }

        protected override void Update()
        {
            m_spells = infocus && sun.active;
            
            base.Update();

            HandleDrag();

            UpdateTrajectory();
            UpdateCursorState();
            UpdateTooltip();
        }
    
        void OnApplicationFocus(bool hasFocus)
        {
            infocus = hasFocus;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            infocus = !pauseStatus;
        }

        #endregion

        #region Interacter
        
        protected override Vector3 origin => cursor.position;

        protected override LayerMask mask
        {
            get
            {
                if (_interaction == Interaction.Drag) 
                    return dragInteractionMask;

                return base.mask;
            }
        }

        protected override void QueryInteractions(out RaycastHit[] hits, out RaycastResult[] hits2d)
        {
            if (spells) 
            {
                base.QueryInteractions(out hits, out hits2d);
            }
            else // Wipe all interactions during frame
            {
                hits = new RaycastHit[]{};
                hits2d = new RaycastResult[]{};
            }
        }

        protected override void HandleInteractions(Dictionary<IInteractable, RaycastHit> _frameInteractions)
        {
            base.HandleInteractions(_frameInteractions);

            if (_frameInteractions.Count() > 0) 
            {
                var _interaction = _frameInteractions.ElementAt(0);
                
                _interactable = _interaction.Key;
                _interactableHit = _interaction.Value;
                _interactableCollider = _interactableHit.collider;
            }
            else 
            {
                _interactable = null;
                _interactableCollider = null;
            }
        }
        
        protected override void onGrabInteractable(IInteractable interactable, RaycastHit hit)
        {
            GrabBeacon(interactable, hit);
        }

        #endregion
        
        #region Beacons
        
        void HandleDrag()
        {
            context = GetContextFromTarget(); // Get drag context
            
            if (_interaction == Interaction.Drag) 
            {
                if (cont) 
                {
                    Vector3 point = Vector3.zero;
                    
                    if (_interactableCollider != null) 
                    {
                        point = _interactableHit.point;
                        pointDistance = _interactableHit.distance;
                    }
                    else 
                    {
                        point = camera.ScreenToWorldPoint(new Vector3(origin.x, origin.y, pointDistance));
                    }
                    
                    beacon.transform.position = point;
                }
                else 
                {
                    pointDistance = defaultPointDistance;
                    
                    if(up) DropBeacon(); // Release beacon
                }
            }
            else 
            {
                pointDistance = defaultPointDistance;
            }
        }

        void GrabBeacon(IInteractable interactable, RaycastHit hit)
        {
            if (_interaction == Interaction.Drag) return;
            
            var _beacon = hit.collider.GetComponent<Beacon>();
            if (_beacon != null && beacon == null) 
            {
                _interaction = Interaction.Drag;
                beacon = _beacon;
                
                beacon.Grab();
            }
        }

        void DropBeacon()
        {
            if (beacon != null) 
            {
                Vector3 origin = _interactableHit.point;

                switch (context) 
                {
                    case DragContext.Addition:
                        beacon.AddToNest();
                        break;
                    case DragContext.Flower:
                        beacon.Flower(origin);
                        break;
                    case DragContext.Plant:
                        beacon.Plant(origin);
                        break;
                    case DragContext.Destroy:
                        beacon.Destroy();
                        break;
                    case DragContext.Release:
                    default:
                        beacon.Release();
                        break;
                }
            }

            _interaction = Interaction.Normal;
            context = DragContext.Release;
            beacon = null;
        }
        
        #endregion

        #region Remote access

        // BEACONS

        public bool AddBeacon(string file, POINT point, bool random = false)
        {
            var @params = new Hashtable();
            
            if (!random) 
            {
                var ray = camera.ScreenPointToRay(new Vector3(point.x, (Screen.height - point.y), 0f));
                var hit = new RaycastHit();

                Debug.LogErrorFormat("Wand attempt to add file => {0} at position => {1}", file, point);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionMask.value)) // Found suitable position 
                {
                    var position = hit.point;
                    @params.Add("position", position);
                    @params.Add("origin", position);
                    
                    beacons.CreateBeacon(file, Beacon.Type.Desktop, Beacon.Locale.Terrain, @params, fromSave: false, transition: BeaconManager.TransitionType.Spawn);
                    return true;
                }
            }
            
            beacons.CreateBeacon(file, Beacon.Type.Desktop, Beacon.Locale.Terrain, @params, fromSave:false, transition: BeaconManager.TransitionType.Spawn);
            return true;
        }

        public bool ActivateBeacon(Beacon beacon) 
        {
            if (beacon == null) return false;

            bool success = beacon.AddToNest();
            if (success)
                Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, Agent, AGENT.Beacon, details: beacon.File);

            return success;
        }

        public bool PlantBeacon(Beacon beacon)
        {
            if (beacon == null) return false;
            return beacon.Plant(beacon.Origin);;
        }
    
        public bool DestroyBeacon(Beacon beacon) 
        {
            if (beacon == null) return false;

            bool success = beacon.Delete();
            if (success)
                Events.ReceiveEvent(EVENTCODE.BEACONDELETE, Agent, AGENT.Beacon, details: beacon.File);

            return success;
        }

        // NEST

        public bool KickNest(Kick kick)
        {
            var nest = Nest.Instance;
        
            print("RL KICK");

            if (kick.useDirection)
                nest.Kick(kick.direction, kick.force, Agent);
            else
                nest.RandomKick(kick.force, Agent);

            return true;
        }

        public bool PopBeaconFromNest(Beacon beacon)
        {
            if (beacon == null) return false;

            bool success = Nest.Instance.RemoveBeacon(beacon);
            if (success)
                Events.ReceiveEvent(EVENTCODE.NESTPOP, Agent, AGENT.Beacon, details: beacon.File);

            return success;
        }

        public bool PopLastBeaconFromNest()
        {
            var beacon = Nest.Instance.RemoveLastBeacon();
            bool success = beacon != null;
            if (success)
                Events.ReceiveEvent(EVENTCODE.NESTPOP, Agent, AGENT.Beacon, details: beacon.File);

            return success;
        }

        public bool ClearNest()
        {
            bool success = Nest.Instance.Dispose();
            if (success)
                Events.ReceiveEvent(EVENTCODE.NESTCLEAR, Agent, AGENT.Nest);

            return success;
        }
    
        // MISCELLANEOUS FUCK MISCELLANEOUS FUQ

        public bool Refocus(Focusable focus)
        {
            if (focus == null) return false;
        
            focus.Focus();
            return true;
        }

        public bool EscapeFocus()
        {
            if (World.IsFocused) 
            {
                var focusing = FindObjectOfType<Focusing>();
                focusing.LoseFocus();
            
                return true;
            }

            return false;
        }

        #endregion

        #region Cursor and trajectory

        void UpdateCursorState()
        {
            if (cursor_icon != null) 
            {
                // Update cursor
                if (_interactable.IsValid())
                    cursor_icon.state = CustomCursor.State.Hover;
                else
                    cursor_icon.state = CustomCursor.State.Normal;
            }
        }

        void UpdateTrajectory()
        {
            Vector2 dir = new Vector2(cursor.velocity.x, cursor.velocity.y);
            dir = dir.normalized * _radius/2f;

            Vector3 t_trajectory2d = position2d + new Vector3(dir.x, dir.y, 0f);
            m_trajectory2d = Vector3.Lerp(m_trajectory2d, t_trajectory2d, Time.deltaTime * 3f);
        }

        #endregion
        
        #region Layers

        DragContext GetContextFromTarget()
        {
            if (_interactableCollider != null) 
            {
                var targetLayer = _interactableCollider.gameObject.layer;

                if (plantMask == (plantMask | (1 << targetLayer)))
                    return DragContext.Plant;
                if (additionMask == (additionMask | (1 << targetLayer)))
                    return DragContext.Addition;
                if (flowerMask == (flowerMask | (1 << targetLayer)))
                    return DragContext.Flower;
                if (destroyMask == (destroyMask | (1 << targetLayer)))
                    return DragContext.Destroy;
            }
            
            return DragContext.Release;
        }
        
        #endregion
    
        #region Tooltips

        void UpdateTooltip()
        {
            if (spells) 
            {
                string message = "";

                if (_interaction == Interaction.Normal) 
                {
                    if (_interactable.IsValid() && _interactable is ITooltip) 
                    {
                        message = ((ITooltip) _interactable).GetInfo();
                    }
                }
                else 
                {
                    if (beacon != null) 
                    {
                        message = beacon.File.AppendContextualInformation(beacon, context, "\n\n");
                    }
                }

                if (string.IsNullOrEmpty(message)) 
                {
                    infoOpacity.Hide();
                }
                else {
                    infoText.text = message;
                    infoOpacity.Show();
                }

                return;
            }
        
            infoOpacity.Hide();
        }
    
        #endregion

        public void Cycle(bool refresh)
        {
            DropBeacon(); 
        }
    }
}