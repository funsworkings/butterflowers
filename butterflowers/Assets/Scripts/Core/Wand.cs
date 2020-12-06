using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using B83.Win32;
using Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using uwu.Animation;
using uwu.Extensions;
using uwu.Gameplay;
using uwu.Snippets;
using uwu.UI.Behaviors.Visibility;
using XNode.Examples.MathNodes;
using Cursor = uwu.Snippets.Cursor;
using Interactable = uwu.Gameplay.Interactable;

public class Wand : Interacter
{
    // External
    
    [SerializeField] WorldPreset preset;
    [SerializeField] ButterflowerManager butterflowers;
    [SerializeField] BeaconManager beacons;

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
        [SerializeField] float brushRadius = 12f;
        [SerializeField] Beacon beacon = null;
        [SerializeField] Entity target = null;
        [SerializeField] float defaultPointDistance = 10f;
        public bool global = true;

        [Header("Debug")] 
            [SerializeField] float pointDistance;
            [SerializeField] Image debugCircle;

    #region Accessors

    protected override Vector3 origin => cursor.position;

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

    public float radius => brushRadius;
    
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
    }

    protected override void Update()
    {
        m_spells = infocus;

        base.Update();
        
        if (spells) 
        {
            UpdateTrajectory();
            HandleBeacon();
        }
        else {
            if (beacon != null) 
                DropBeacon();
        }
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

    protected override void HandleInteractions(Dictionary<uwu.Gameplay.Interactable, RaycastHit> _frameInteractions)
    {
        if (spells) 
        {
            base.HandleInteractions(_frameInteractions);

            var hits = _frameInteractions.Values.ToArray();
            UpdateCursorState(hits);
        }
        else 
        {
            UpdateCursorState(null);    
        }
    }

    protected override void FilterInteractions(ref Dictionary<uwu.Gameplay.Interactable, RaycastHit> _frameInteractions)
    {
        ExcludeDistantInteractables<Beacon>(ref _frameInteractions);
        ExcludeDistantInteractables<Vine>(ref _frameInteractions);
    }

    void ExcludeDistantInteractables<E>(ref Dictionary<uwu.Gameplay.Interactable, RaycastHit> _frameInteractions) where E:MonoBehaviour
    {
        var interactions_temp = _frameInteractions.Keys;
        
        IEnumerable<uwu.Gameplay.Interactable> typed_int = FilterInteractablesByType<E>(interactions_temp);
        uwu.Gameplay.Interactable closest_int = FindClosestInteractable(typed_int.ToList());

        foreach (uwu.Gameplay.Interactable i in typed_int) {
            if (i != closest_int) _frameInteractions.Remove(i); // Remove distant type item
        }
    }

    protected override void onGrabInteractable(uwu.Gameplay.Interactable interactable)
    {
        var beacon = interactable.GetComponent<Beacon>();
        if (beacon != null && this.beacon == null) 
        {
            this.beacon = beacon;
            beacon.Grab();
        }
    }

    protected override void onReleaseInteractable(uwu.Gameplay.Interactable interactable)
    {
        var entity = interactable.GetComponent<Entity>();
        if (entity != null) 
        {
            if (entity is Terrain) 
            {
                
            }
            else if (entity is Nest) 
            {
                
            }
        }
    }

    #endregion

    #region Remote access

    // BEACONS

    public bool AddBeacon(string file, POINT point)
    {
        var ray = camera.ScreenPointToRay(new Vector3(point.x, (Screen.height - point.y), 0f));
        var hit = new RaycastHit();
 
         Debug.LogErrorFormat("Wand attempt to add file => {0} at position => {1}", file, point);
         
         if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionMask.value)) // Found suitable position 
         {
             var position = hit.point;
             beacons.CreateBeaconInstance(file, Beacon.Type.Desktop, position, true);
         }
         else 
         {
             beacons.CreateBeaconInstance(file, Beacon.Type.Desktop, Vector3.zero, false);
         }
 
         return false;
    }

    public bool ActivateBeacon(Beacon beacon) 
    {
        if (beacon == null) return false;

        bool success = beacon.Activate();
        if (success)
            Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, Agent, AGENT.Beacon, details: beacon.file);

        return success;
    }

    public bool PlantBeacon(Beacon beacon)
    {
        if (beacon == null) return false;

        bool success = beacon.Plant();
        if(success)
            Events.ReceiveEvent(EVENTCODE.BEACONPLANT, Agent, AGENT.Beacon, details: beacon.file);

        return success;
    }
    
    public bool DestroyBeacon(Beacon beacon) 
    {
        if (beacon == null) return false;

        bool success = beacon.Delete(particles: true);
        if (success)
            Events.ReceiveEvent(EVENTCODE.BEACONDELETE, Agent, AGENT.Beacon, details: beacon.file);

        return success;
    }

    void HandleBeacon()
    {
        if (beacon != null) 
        {
            if (cont) 
            {
                Vector3 point = Vector3.zero;
                if (Physics.Raycast(ray, out raycastHit, interactionDistance, interactionMask.value)) 
                {
                    point = raycastHit.point;
                    pointDistance = raycastHit.distance;

                    target = raycastHit.collider.GetComponent<Entity>();
                }
                else {
                    point = camera.ScreenToWorldPoint(new Vector3(origin.x, origin.y, pointDistance));
                    target = null;
                }

                beacon.transform.position = point;
            }
            else 
            {
                pointDistance = defaultPointDistance;
                if(up) DropBeacon();
            }
        }
        else {
            pointDistance = defaultPointDistance;
        }
    }

    void DropBeacon()
    {
        if (target is Terrain) 
        {
            Vector3 origin = raycastHit.point;
            beacon.PlantAtLocation(origin);
        }
        else if (target is Nest)
            beacon.Activate();
        else
            beacon.Drop();
        
        beacon = null;
        target = null;
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
            Events.ReceiveEvent(EVENTCODE.NESTPOP, Agent, AGENT.Beacon, details: beacon.file);

        return success;
    }

    public bool PopLastBeaconFromNest()
    {
        var beacon = Nest.Instance.RemoveLastBeacon();
        bool success = beacon != null;
        if (success)
            Events.ReceiveEvent(EVENTCODE.NESTPOP, Agent, AGENT.Beacon, details: beacon.file);

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

    void UpdateCursorState(RaycastHit[] hits)
    {
        if (cursor_icon != null) 
        {
            if (World.IsRemote || !spells) 
            {
                cursor_icon.state = CustomCursor.State.Remote;
                return;
            }
            
            // Update cursor
            if (hits != null && hits.Length > 0)
                cursor_icon.state = CustomCursor.State.Hover;
            else
                cursor_icon.state = CustomCursor.State.Normal;
        }
    }

    void UpdateTrajectory()
    {
        Vector2 dir = new Vector2(cursor.velocity.x, cursor.velocity.y);
        dir = dir.normalized * brushRadius/2f;

        Vector3 t_trajectory2d = position2d + new Vector3(dir.x, dir.y, 0f);
        m_trajectory2d = Vector3.Lerp(m_trajectory2d, t_trajectory2d, Time.deltaTime * 3f);
    }

	#endregion
    
    #region Debug
    
    void UpdateDebugCircle()
    {
        Color col = (global)? Color.green:Color.yellow;

        debugCircle.color = new Color(col.r, col.g, col.b, .33f);
        debugCircle.rectTransform.sizeDelta = new Vector2(brushRadius*2f, brushRadius*2f);
        debugCircle.rectTransform.position = trajectory2d;
    }
    
    #endregion
}