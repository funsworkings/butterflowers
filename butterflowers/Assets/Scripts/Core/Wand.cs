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
using uwu.Snippets;
using uwu.UI.Behaviors.Visibility;
using XNode.Examples.MathNodes;
using Cursor = uwu.Snippets.Cursor;

public class Wand : Entity
{
    // Events
    
    [SerializeField] WorldPreset preset;
    [SerializeField] ButterflowerManager butterflowers;
    [SerializeField] BeaconManager beacons;

    #region Internal

    public enum State
    {
        Drift,
        Paint
    }
    
    #endregion
    
    // Events

    public System.Action<Gesture> onGestureBegin, onGestureEnd;
    public UnityEvent GestureStart, GestureEnd;

    // Attributes

    [Header("General")]
	    [SerializeField] new Camera camera;
        [SerializeField] bool m_spells = true;

        [SerializeField] Cursor cursor;
        [SerializeField] float distanceFromCamera = 10f;
        [SerializeField] float cursorToWorldRate = 1f;

        public State state = State.Drift;
        public bool infocus = true;

        SimpleVelocity _simpleVelocity;

    Animation animator;
    Gestures Gestures;

    [Header("Interaction")]
        Ray ray;
        RaycastHit raycastHit;
       
        [SerializeField] 
        LayerMask interactionMask, navigationMask;
        [SerializeField]
        float interactionDistance = 100f;

        [SerializeField] bool multipleInteractions = true;

        [SerializeField] float brushDistance = 10f;
        [SerializeField] float brushSpeed = 1f;
        [SerializeField] float brushRadius = 12f;
        
        [SerializeField] List<uwu.Gameplay.Interactable> interacting = new List<uwu.Gameplay.Interactable>();

        [SerializeField] bool down, cont, up;
         [SerializeField] CustomCursor cursor_icon;

         [SerializeField] float paint_t = 0f;
         [SerializeField] float paintInterval = 1f;

         [SerializeField] bool include2d = false;

         public bool global = true;

         [Header("Gestures")] 
             [SerializeField] Gesture m_queueGesture;
             [SerializeField] Gesture m_currentGesture;
             [SerializeField] bool gesture = false, waitforgesture = false;

        [Header("Debug")] 
            [SerializeField] Image debugCircle;
            [SerializeField] Vector3 pushFromCamera;
            [SerializeField] float pushStrength = 1f;

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

    public bool gestureInProgress => animator.isPlaying;

    public float radius => brushRadius;
    
    public Camera Camera => camera;

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

    public bool debug = true;

    #region Monobehaviour callbacks

    void Awake()
    {
        animator = GetComponent<Animation>();
        Gestures = GetComponent<Gestures>();
        _simpleVelocity = GetComponent<SimpleVelocity>();
    }

	// Start is called before the first frame update
	protected override void OnStart()
    {
        base.OnStart();
        
        if(camera == null)
            camera = Camera.main;
    }

    protected override bool EvaluateUpdate()
    {
        return true;
    }

    protected override void OnUpdate()
    {
        if (camera == null)
            return;

        m_spells = infocus;

        if (spells) 
        {
            down = Input.GetMouseButtonDown(0);
            cont = Input.GetMouseButton(0);
            up = Input.GetMouseButtonUp(0);

            Interact();

            if (Input.GetKeyDown(KeyCode.X)) {
                butterflowers.KillButterfliesInDirection(camera.transform.TransformDirection(pushFromCamera.normalized),
                    pushStrength);
            }

            UpdateTrajectory();
            if (debugCircle != null) {
                if (debug) {
                    debugCircle.enabled = true;
                    UpdateDebugCircle();
                }
                else
                    debugCircle.enabled = false;
            }
        }
        else {
            UpdateCursorState(null, null);
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

    #region Interaction

	void Interact()
    {
        ray = camera.ScreenPointToRay(cursor.position);

        var hits = new RaycastHit[] { };
        if (multipleInteractions)
            hits = Physics.RaycastAll(ray, interactionDistance, interactionMask.value);
        else {
            var hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, interactionDistance, interactionMask.value)) 
            {
                hits = new RaycastHit[]{ hit };       
            }
        }

        if (!Sun.Instance.active) {
            hits = new RaycastHit[] { };
        }

        var ray_2d = new PointerEventData(EventSystem.current);
            ray_2d.position = Input.mousePosition;
        var hits_2d = new List<RaycastResult>();

        EventSystem.current.RaycastAll(ray_2d, hits_2d);

        if (!spells) 
        {
            hits = new RaycastHit[] { }; // Discard all raycast events if no spells
            //hits_2d = new List<RaycastResult>();
        }
        
        if(!include2d)
            hits_2d = new List<RaycastResult>(); // Wipe out 2d interactions

        ParseInteractions(hits, hits_2d.ToArray());
    }

    void ParseInteractions(RaycastHit[] hits, RaycastResult[] hits_2d)
    {
        List<uwu.Gameplay.Interactable> interacting = new List<uwu.Gameplay.Interactable>();

        if(hits != null && hits_2d != null){
            GameObject obj = null;
            uwu.Gameplay.Interactable obj_int = null;

            Dictionary<uwu.Gameplay.Interactable, RaycastHit> temp_int = new Dictionary<uwu.Gameplay.Interactable, RaycastHit>();

            foreach(RaycastHit hit in hits)
            {
                obj = hit.collider.gameObject;
                obj_int = obj.GetComponent<uwu.Gameplay.Interactable>();

                if (obj_int != null) 
                    temp_int.Add(obj_int, hit);

                //Debug.LogFormat("Collided with {0}", hit.collider.gameObject.name);
            }

            List<uwu.Gameplay.Interactable> interactions_3d = temp_int.Keys.ToList();
            
            FilterClosestInteractables<Beacon>(ref interactions_3d);
            FilterClosestInteractables<Vine>(ref interactions_3d);

            foreach (uwu.Gameplay.Interactable i in interactions_3d) 
            {
                var hit = temp_int[i];

                if (down)
                    i.Grab(hit);
                else if (cont)
                    i.Continue(hit);
                else if (up)
                    i.Release(hit);
                else {
                    i.Hover(hit);
                    interacting.Add(i);
                }
            }


            var ray_hit = new RaycastHit();
            foreach (RaycastResult hit_2d in hits_2d) 
            {
                obj = hit_2d.gameObject;
                obj_int = obj.GetComponent<uwu.Gameplay.Interactable>();

                if (obj_int != null) {
                    ray_hit.point = obj.transform.position;
                    ray_hit.normal = obj.transform.forward;

                    if (down)
                        obj_int.Grab(ray_hit);
                    else if (cont)
                        obj_int.Continue(ray_hit);
                    else if (up)
                        obj_int.Release(ray_hit);
                    else {
                        obj_int.Hover(ray_hit);
                        interacting.Add(obj_int);
                    }
                }
            }

            UpdateCursorState(hits, hits_2d);
        }

        ParseLastInteractions(interacting);
    }

    uwu.Gameplay.Interactable GetClosestInteractable(List<uwu.Gameplay.Interactable> hovering)
    {
        float minDistance = Mathf.Infinity;
        uwu.Gameplay.Interactable closest = null;

        foreach (uwu.Gameplay.Interactable i in hovering) {
            float d = Vector3.Distance(transform.position, i.transform.position);
            if (d <= minDistance) {
                minDistance = d;
                closest = i;
            }
        }

        return closest;
    }

    void FilterClosestInteractables<E>(ref List<uwu.Gameplay.Interactable> interactions_temp) where E:MonoBehaviour
    {
        IEnumerable<uwu.Gameplay.Interactable> typed_int = interactions_temp.Where(i => i.GetComponent<E>() != null);

        uwu.Gameplay.Interactable closest_int = GetClosestInteractable(typed_int.ToList());
        if (closest_int != null) 
        {
            typed_int = typed_int.Except(new uwu.Gameplay.Interactable[] { closest_int });
            interactions_temp = interactions_temp.Except(typed_int).ToList();
        }
    }

    void ParseLastInteractions(List<uwu.Gameplay.Interactable> current){
        for(int i = 0; i < interacting.Count; i++)
        {
            var interactable = interacting[i];
            if(!current.Contains(interactable))
                interactable.Unhover();
        }

        interacting = current;
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
            beacons.CreateBeaconInstance(file, Beacon.Type.Desktop, position);

            return true;
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

    void UpdateCursorState(RaycastHit[] hits, RaycastResult[] hits_2d)
    {
        if (cursor_icon != null) 
        {
            if (World.Remote || !spells) 
            {
                cursor_icon.state = CustomCursor.State.Remote;
                return;
            }
            
            // Update cursor
            if (hits.Length > 0 || hits_2d.Length > 0)
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