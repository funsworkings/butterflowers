using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Wand : MonoBehaviour
{
    #region Events

    public System.Action<Gesture> onGestureBegin, onGestureEnd;
    public UnityEvent GestureStart, GestureEnd;

	#endregion

	new Camera camera;

    public AGENT agent = AGENT.Inhabitant0;
    [SerializeField] bool m_spells = true;

    [SerializeField] Cursor cursor;
    [SerializeField] float distanceFromCamera = 10f;

    Animation animator;
    Gestures Gestures;

    [Header("Interaction")]
        Ray ray;
        RaycastHit raycastHit;
       
        [SerializeField] 
        LayerMask interactionMask, navigationMask;
        [SerializeField]
        float interactionDistance = 100f;

        [SerializeField] List<Interactable> interacting = new List<Interactable>();

        [SerializeField] bool down, cont, up;

    [Header("Gestures")]
        [SerializeField] Gesture m_queueGesture, m_currentGesture;
        [SerializeField] bool gesture = false, waitforgesture = false;

    public Vector3 position {
        get{
            return cursor.position;
        }
        set {
            transform.position = value;
        }
    }

    public Vector3 worldPosition {
        get
        {
            if (camera == null)
                return Vector3.zero;

            return camera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10f));
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

    public Vector3 velocity {
        get{
            return cursor.velocity;
        }
    }

    public float speed {
        get{
            return cursor.speed;
        }
    }

    public bool inprogress => animator.isPlaying;

    #region Internal

    [System.Serializable]
    public struct Kick 
    {
        public bool useDirection;
        public Vector3 direction;
    }

    #endregion

    #region Monobehaviour callbacks

    void Awake()
    {
        animator = GetComponent<Animation>();
        Gestures = GetComponent<Gestures>();
    }

	// Start is called before the first frame update
	void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        if (camera == null)
            return;

        down = Input.GetMouseButtonDown(0);
        cont = Input.GetMouseButton(0);
        up = Input.GetMouseButtonUp(0);

        Interact();

        if (cont) Waypoint();
    }

    #endregion

    #region Gestures

    public bool EnactGesture(Gesture t_gesture, float speed = 1f)
    {
        if (animator == null) return false;
        if (gesture || waitforgesture) return false;

        Gestures.PlayAnimation(t_gesture.clip);

        waitforgesture = true;
        m_queueGesture = t_gesture;

        StartCoroutine("TimeoutGesture");
        BeginGesture();

        return true;
    }

    public bool CancelGesture()
    {
        if (!inprogress) return false;

        Gestures.StopAnimation();
        return true;
    }

    IEnumerator TimeoutGesture()
    {
        float t = 0f;
        float timeout = .167f;

        while (t < timeout && waitforgesture) 
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (t < timeout) {
            Debug.Log("Gesture SUCCESS!");

            m_currentGesture = m_queueGesture;
            m_queueGesture = null;

            GestureStart.Invoke();
            if (onGestureBegin != null)
                onGestureBegin(m_currentGesture);

            gesture = spells = true;
        }
        else {
            Debug.Log("Gesture FAIL!");

            m_queueGesture = m_currentGesture = null;
            gesture = spells = false;

            GestureEnd.Invoke();
        }

        waitforgesture = false;

        while (inprogress)
            yield return null;

        EndGesture();
    }

    public void BeginGesture()
    {
        if (waitforgesture) 
            waitforgesture = false;
    }

    public void EndGesture()
    {
        GestureEnd.Invoke();
        if (onGestureEnd != null)
            onGestureEnd(m_currentGesture);

        m_queueGesture = m_currentGesture = null;

        gesture = false;
        spells = false;
    }

	#endregion

	#region Navigation

	void Waypoint()
    {
        ray = camera.ScreenPointToRay(position);

        var hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, interactionDistance, navigationMask.value)){
            //wizard.MoveTo(hit.point); // Move wizard target to location
        }
    }

	#endregion

	#region Interaction

	void Interact()
    {
        ray = camera.ScreenPointToRay(position);

        var hits = Physics.RaycastAll(ray, interactionDistance, interactionMask.value);
        ParseInteractions(hits);
    }

    void ParseInteractions(RaycastHit[] hits)
    {
        List<Interactable> interacting = new List<Interactable>();

        if(hits != null){
            GameObject obj = null;
            Interactable obj_int = null;

            foreach(RaycastHit hit in hits)
            {
                obj = hit.collider.gameObject;
                obj_int = obj.GetComponent<Interactable>();

                if(obj_int != null)
                {
                    if (down)
                        obj_int.Grab(hit);
                    else if (cont)
                        obj_int.Continue(hit);
                    else if (up)
                        obj_int.Release(hit);
                    else {
                        obj_int.Hover(hit);
                        interacting.Add(obj_int);
                    }
                }

                //Debug.LogFormat("Collided with {0}", hit.collider.gameObject.name);
            }
        }

        ParseLastInteractions(interacting);
    }

    void ParseLastInteractions(List<Interactable> current){
        for(int i = 0; i < interacting.Count; i++){
            var interactable = interacting[i];
            if(!current.Contains(interactable))
                interactable.Unhover();
        }

        interacting = current;
    }

    #endregion

    #region Beacon operations

    public void ActivateBeacon(Beacon beacon) 
    {
        if (beacon == null) return;

        bool success = beacon.Discover();
        if (success)
            Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, agent, AGENT.Beacon, details: beacon.file);
    }
    public void DestroyBeacon(Beacon beacon) 
    {
        if (beacon == null) return;

        bool success = beacon.Delete(particles: true);
        if (success)
            Events.ReceiveEvent(EVENTCODE.BEACONDELETE, agent, AGENT.Beacon, details: beacon.file);
    }

    #endregion

    #region Nest operations

    public void KickNest() 
    { 
        Nest.Instance.RandomKick(agent); 
    }

    public void KickNest(Kick kick)
    {
        var nest = Nest.Instance;

        if (kick.useDirection)
            nest.Kick(kick.direction, agent);
        else
            nest.RandomKick(agent);
    }

    public void PopBeaconFromNest(Beacon beacon) 
    {
        if (beacon == null) return;

        bool success = Nest.Instance.RemoveBeacon(beacon);
        if (success)
            Events.ReceiveEvent(EVENTCODE.NESTPOP, agent, AGENT.Beacon, details: beacon.file);
    }

    public void PopLastBeaconFromNest() 
    { 
        var beacon = Nest.Instance.RemoveLastBeacon();
        bool success = beacon != null;
        if (success)
            Events.ReceiveEvent(EVENTCODE.NESTPOP, agent, AGENT.Beacon, details: beacon.file);
    }

    public void ClearNest() 
    {

        bool success = Nest.Instance.Dispose();
        if(success)
            Events.ReceiveEvent(EVENTCODE.NESTCLEAR, agent, AGENT.Nest);
    }

    #endregion
}