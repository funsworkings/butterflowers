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
using Objects.Base;
using Objects.Entities.Interactables.Empty;
using Behaviour = AI.Types.Behaviour;

public class Beacon: Interactable {

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
        Destroyed,
        Drag,
        Flower
    }

	#endregion
    
    // External
    
    World Room;
    Nest Nest;

    // Events

	public static System.Action<Beacon> OnRegister, OnUnregister;
    public static System.Action<Beacon> Activated, Destroyed, Planted, Flowered;

    // Properties

    [SerializeField] WorldPreset preset;
    [SerializeField] ParticleSystem deathPS, addPS;

    SimpleOscillate Oscillate;
    new MeshRenderer renderer;
    new Collider collider;
    Material material;

    [SerializeField] GameObject pr_impactPS, pr_shinePS;
    [SerializeField] GameObject pr_flower;
    
    public Type type = Type.None;
    public Locale state = Locale.Terrain;
    public AGENT parent = AGENT.NULL;
    
    [SerializeField] string m_file = null;
    [SerializeField] FileSystemEntry m_fileEntry;
    public BeaconInfo beaconInfo;
    
    public Vector3 origin = Vector3.zero;
    public Vector3 size = Vector3.one;

    // Attributes
    
    [SerializeField] bool hovered = false, m_discovered = false;

    [SerializeField] bool returnToOrigin = false;
    [SerializeField] Vector3 releasePosition = Vector3.zero, releaseScale = Vector3.zero;
    [SerializeField] float lerp_t = 0f, lerp_duration = 1f;
    AnimationCurve scaleCurve;
    
    
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
            m_discovered = value;
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

    protected override void Update()
    {
        if (!Active) HideTooltip();
        UpdateColor();
        
        base.Update();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // Interactions on hover (E / P)
        if (hovered)
            EvaluateState();

        if (returnToOrigin) 
        {
            lerp_t += Time.deltaTime;
            ReturnToOrigin(lerp_t);
        }
        
        EvaluateState();
    }

    void OnDisable() {
        Unregister();
    }


    #endregion

    #region Interactable callbacks

    protected override void onHover(Vector3 point, Vector3 normal)
    {
        if (!Active) return;
        hovered = true;

        base.onHover(point, normal);
    }

    protected override void onUnhover(Vector3 point, Vector3 normal)
    {
        if (!Active) return;
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

    public void Initialize(Type type, Locale state, Vector3 origin, Transform tooltipContainer)
    {
        Room = World.Instance;
        Nest = Nest.Instance;

        collider = GetComponent<Collider>();
        renderer = GetComponentInChildren<MeshRenderer>();
        material = renderer.material;

        CreateTooltip(tooltipContainer);

        this.type = type;
        this.state = state;
        this.origin = origin;
        
        this.size = preset.normalBeaconScale * Vector3.one;
        this.lerp_duration = preset.beaconLerpDuration;
        this.scaleCurve = preset.beaconScaleCurve;

        ReturnToOrigin(99f);
        if(state == Locale.Flower) CreateFlowerAtOrigin();
    }


    #endregion

    #region Operations

    public bool Activate() 
    {
        if (state != Locale.Drag) return false;
        state = Locale.Nest;

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play(); // Trigger particle sys

        addPS.Play();
        deathPS.Stop();
        
        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        transform.localScale = Vector3.zero;

        Events.ReceiveEvent(EVENTCODE.BEACONACTIVATE, AGENT.User, AGENT.Beacon, details: file);
        if (Activated != null)
            Activated(this);

        return true;
    }

    public void PlantAtLocation(Vector3 point)
    {
        if (state != Locale.Drag) return;
        
        origin = point;
        Plant();
    }

    public bool Plant()
    {
        if (state != Locale.Drag) return false;
        state = Locale.Planted;

        transform.localScale = Vector3.zero;

        Events.ReceiveEvent(EVENTCODE.BEACONPLANT, AGENT.User, AGENT.Beacon, details: file);
        if (Planted != null)
            Planted(this);

        return true;
    }

    public void FlowerAtLocation(Vector3 point)
    {
        if (state != Locale.Drag) return;

        origin = point;
        Flower();
    }

    public bool Flower()
    {
        if (state != Locale.Drag) return false;
        state = Locale.Flower;

        transform.localScale = Vector3.zero;
        
        CreateFlowerAtOrigin();

        Events.ReceiveEvent(EVENTCODE.BEACONFLOWER, AGENT.User, AGENT.Beacon, details: file);
        if (Flowered != null)
            Flowered(this);
        
        return true;
    }

    void CreateFlowerAtOrigin()
    {
        var flowerInstance = Instantiate(pr_flower, origin, Quaternion.identity);
        var flower = flowerInstance.GetComponentInChildren<Flower>();
            flower.Grow(Objects.Entities.Interactables.Empty.Flower.GrowProfile.Small, file, type);
    }

    public bool Deactivate()
    {
        Debug.LogFormat("Deactivate {0} when state is {1}", file, state);
        if (state != Locale.Nest) return false;
        state = Locale.Terrain;

        deathPS.Play();
        addPS.Stop();

        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        transform.localScale = Vector3.zero;
        
        ReturnToOrigin(-1f, initial:true);

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

        return true;
    }

    public void Grab()
    {
        if (state != Locale.Terrain) return;
        state = Locale.Drag;
    }

    public void Drop()
    {
        if (state != Locale.Drag) return;
        state = Locale.Terrain;
        
        ReturnToOrigin(-1f, initial:true);
    }

    #endregion

    #region State

    void EvaluateState()
    {
        renderer.enabled = true;

        if (state == Locale.Terrain && !returnToOrigin) 
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

    #region Appearance

    void UpdateColor() 
    {
        if (material == null) return;

        float hue = 0f;
        float sat = 0f, t_sat = (((Element) this).Active) ? 1f : 0f;
        float val = 0f;

        Color.RGBToHSV(material.color, out hue, out sat, out val);
        Color actual = Color.HSVToRGB(hue, Mathf.Lerp(sat, t_sat, Time.deltaTime), val);
        actual.a = material.color.a;
        
        material.color = actual;
    }

    #endregion
    
    #region Element overrides

    protected override bool EvaluateActiveState()
    {
        return Sun.active && Nest.open;
    }

    #endregion

    #region Movement

    void ReturnToOrigin(float duration, bool initial = false)
    {
        if (initial) 
        {
            returnToOrigin = true;
            releasePosition = transform.position;
            releaseScale = transform.localScale;
            lerp_t = 0f;
            
            return;
        }

        float i = duration / lerp_duration;

        bool complete = (i >= 1f);
        i = Mathf.Clamp01(i); // [0-1]

        Vector3 dest_pos = origin;
        
        Vector3 scaleA = releaseScale;
        Vector3 scaleB = size;

        transform.localScale = Vector3.Lerp(scaleA, scaleB, scaleCurve.Evaluate(i));
        transform.position = Vector3.Lerp(releasePosition, dest_pos, i);

        if (complete) {
            returnToOrigin = false; // Stop lerping if arrived
            deathPS.Stop();
        }
    }

	#endregion

    #region Info

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
