using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using Noder.Nodes.Abstract;
using Settings;
using uwu.IO.SimpleFileBrowser.Scripts;
using uwu.Snippets;
using XNode.Examples.MathNodes;
using AI;
using Behaviour = AI.Types.Behaviour;

public class Beacon: Interactable {

    #region External

    BeaconManager Manager;
	World Room;
    Nest Nest;

    #endregion

    #region Internal

    public enum Type {
        None,
        Desktop,
        Wizard,
        External
    }

    public enum Status 
    {
        NULL,

        UNKNOWN,
        COMFORTABLE,
        ACTIONABLE
    }

    public Status status => (Room == null)? Status.NULL:Room.FetchBeaconStatus(this);

    public enum Locale 
    {
        Terrain,
        Nest,
        Planted,
        Destroyed
    }

	#endregion

	#region Events

	public static System.Action<Beacon> OnRegister, OnUnregister;
    public static System.Action<Beacon> Discovered, Undiscovered, Destroyed, Planted;

    #endregion

    #region Properties

    [SerializeField] ParticleSystem revealPS, discoveryPS, deathPS, addPS, auraPS;

    SimpleOscillate Oscillate;
    new MeshRenderer renderer;
    new Collider collider;
    Material material;

    [SerializeField] GameObject pr_impactPS, pr_shinePS;

    #endregion

    #region Attributes

    public Type type = Type.None;
    public Locale state = Locale.Terrain;

    bool active = false;
    bool hovered = false;

    [SerializeField] bool m_discovered = false;

    public BeaconInfo beaconInfo;

    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;

    public Transform parent;
    public Vector3 origin = Vector3.zero;
    public Vector3 size = Vector3.one;

    [SerializeField] bool lerp = false, statechange = false;
    [SerializeField] float lerp_t = 0f, lerp_duration = 1f;
    AnimationCurve scaleCurve;

    [SerializeField] float timeToDie = 1.67f;

    public bool learning = false;

    #endregion

    #region Accessors

    public string file {
        get
        {
            return m_file;
        }
        set
        {
            m_file = value;
            RefreshInfo();
        }
    }

    public FileSystemEntry fileEntry {
        get
        {
            return m_fileEntry;
        }
        set
        {
            m_fileEntry = value;
            RefreshInfo();
        }
    }

    public bool discovered {
        get
        {
            return m_discovered;
        }
        set
        {
            if (value)
                Discover(false);
            else
                Hide(false);
        }
    }

    public float knowledge 
    {
        get
        {
            return (Room == null)? 1f:Room.FetchBeaconKnowledgeMagnitude(this);
        }
    }
    
    public bool visible => state == Locale.Terrain;

    #endregion

    #region Monobehaviour callbacks

    protected override void Awake() 
    {
        base.Awake();

        Oscillate = GetComponent<SimpleOscillate>();
    }

    protected override void OnUpdate()
    {
        active = Nest.open;
        if (!active) HideTooltip();

        UpdateColor();
        
        base.OnUpdate();

        // Interactions on hover (E / P)
        if (hovered) 
        {
            if (Input.GetKeyUp(KeyCode.A))
                Activate();
            else if (Input.GetKeyUp(KeyCode.P))
                Plant();

            EvaluateState();
        }

        if (lerp) 
        {
            if (!statechange) 
            {
                lerp_t += Time.deltaTime;
                LerpToDestination(lerp_t);
            }
        }
        else
            EvaluateState();
        
        if (statechange) // Wipe state change during frame
            statechange = false;
    }

    void OnEnable() 
    {
        Beacon.Discovered += CheckIfDuplicateDiscovery;
    }

    void OnDisable() {
        Beacon.Discovered -= CheckIfDuplicateDiscovery;

        Unregister();
    }


    #endregion

    #region Interactable callbacks

    protected override void onHover(Vector3 point, Vector3 normal)
    {
        if (!active) return;
        hovered = true;

        base.onHover(point, normal);
    }

    protected override void onUnhover(Vector3 point, Vector3 normal)
    {
        if (!active) return;
        hovered = false;

        base.onUnhover(point, normal);
    }

	#endregion

	#region Registration & Initialization

	public void Register()
    {
        if (OnRegister != null)
            OnRegister(this);
    }

    public void Unregister()
    {
        if (OnUnregister != null)
            OnUnregister(this);
    }

    public void Initialize(Type type, Locale state, Vector3 origin, Vector3 scale, Transform parent, float lerpTime, AnimationCurve scaling, Transform tooltipContainer)
    {
        Manager = BeaconManager.Instance;
        Room = World.Instance;
        Nest = Nest.Instance;

        collider = GetComponent<Collider>();
        renderer = GetComponentInChildren<MeshRenderer>();
        material = renderer.material;

        CreateTooltip(tooltipContainer);

        this.type = type;
        this.state = state;
        this.origin = origin;

        this.parent = parent;
        this.size = scale;
        this.lerp_duration = lerpTime;
        this.scaleCurve = scaling;

        LerpToDestination(99f);
    }


    #endregion

    #region Operations

    public bool Activate() 
    {
        if (state != Locale.Terrain) return false;
        
        state = Locale.Nest;
        statechange = true;

        Discover();
        
        Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.User, AGENT.Beacon, details: file);

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play(); // Trigger particle sys

        addPS.Play();
        deathPS.Stop();
        
        lerp = true;
        lerp_t = 0f;

        return true;
    }

    public bool Plant()
    {
        if (state != Locale.Terrain) return false;
        
        state = Locale.Planted;
        statechange = true;

        lerp = true;
        lerp_t = 0f;
        
        Events.ReceiveEvent(EVENTCODE.BEACONPLANT, AGENT.User, AGENT.Beacon, details: file);

        if (Planted != null)
            Planted(this);

        return true;
    }

    public bool Deactivate()
    {
        Debug.LogFormat("Deactivate {0} when state is {1}", file, state);
        if (state != Locale.Nest) return false;
        
        state = Locale.Terrain;
        statechange = true;
        
        deathPS.Play();
        addPS.Stop();

        lerp = true;
        lerp_t = 0f;

        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp

        return true;
    }

    public bool Delete(bool events = true, bool particles = false)
    {
        if (state == Locale.Destroyed) return false;
        state = Locale.Destroyed;

        Unregister();
        HideTooltip();

        if (Destroyed != null && events) 
        {
            Destroyed(this);
        }
        
        Destroy(gameObject);
        //StartCoroutine("Dying", particles);

        return true;
    }

    #endregion

    #region State

    void EvaluateState()
    {
        renderer.enabled = true;

        if (state == Locale.Terrain) 
        {
            Oscillate.enabled = false;
            interactive = true;
            collider.enabled = true;
        }
        else 
        {
            Oscillate.enabled = false;
            interactive = false;
            collider.enabled = false;
        }
    }

    #endregion

    #region Discovery

    // Simpler accessors

    public void Discover(bool events = true) { ToggleDiscovery(true, events); }
    public void Hide(bool events = true) { ToggleDiscovery(false, events); }

    void ToggleDiscovery(bool discovered, bool events = true)
    {
        m_discovered = discovered;

        if (events) 
        {
            if (discovered && Discovered != null)
                Discovered(this);

            if (!discovered && Undiscovered != null)
                Undiscovered(this);
        }

        if (discovered) discoveryPS.Stop();
        else discoveryPS.Play();
    }

	#endregion

	#region Death

	IEnumerator Dying(bool particles = false)
    {
        float t = 0f;
        float sp = Oscillate.speed;

        if (particles) 
        {
            deathPS.Play();
            deathPS.transform.parent = null;

            yield return new WaitForSeconds(timeToDie);
        }
        
        Destroy(gameObject);
    }

    #endregion

    #region Appearance

    void UpdateColor() 
    {
        if (material == null) return;

        float hue = 0f;
        float sat = 0f, t_sat = (active) ? 1f : 0f;
        float val = 0f;

        Color.RGBToHSV(material.color, out hue, out sat, out val);
        Color actual = Color.HSVToRGB(hue, Mathf.Lerp(sat, t_sat, Time.deltaTime), val);
        actual.a = material.color.a;
        
        material.color = actual;
    }

    #endregion

    #region Movement

    void LerpToDestination(float duration)
    {
        float i = duration / lerp_duration;

        bool complete = (i >= 1f);
        i = Mathf.Clamp01(i); // [0-1]

        Vector3 dest_pos = Vector3.zero;
        
        Vector3 scaleA = Vector3.zero;
        Vector3 scaleB = Vector3.zero;

        if (state == Locale.Terrain) {
            dest_pos = origin;

            scaleA = Vector3.zero;
            scaleB = size;
        }
        else if (state == Locale.Nest) {
            dest_pos = Nest.transform.position;

            scaleA = size;
            scaleB = Vector3.zero;

            if (complete) 
                Nest.ReceiveBeacon(this); // Receive beacon in nest
        }
        else if (state == Locale.Planted) {
            dest_pos = origin;

            scaleA = size; 
            scaleB = Vector3.zero;
        }

        transform.localScale = Vector3.Lerp(scaleA, scaleB, scaleCurve.Evaluate(i));
        transform.position = Vector3.Lerp(transform.position, dest_pos, i);

        if (complete) 
            lerp = false; // Stop lerping if arrived
    }

	#endregion

	#region Beacon callbacks

	void CheckIfDuplicateDiscovery(Beacon beacon)
    {
        if (beacon == null || beacon == this) return;

        if (beacon.file == file) // Matches file
        { 
            Discover(false); // No fire events
        }
    }

    #endregion

    #region Info operations

    protected override void CreateTooltip(Transform container = null)
    {
        base.CreateTooltip(container);

        beaconInfo = tooltipInstance.GetComponent<BeaconInfo>();
        UpdateTooltipText(beaconInfo.parseTextFromBeacon(this));
    }

    protected void RefreshInfo()
    {
        if(beaconInfo != null)
            UpdateTooltipText(beaconInfo.parseTextFromBeacon(this));
    }

    #endregion
}
