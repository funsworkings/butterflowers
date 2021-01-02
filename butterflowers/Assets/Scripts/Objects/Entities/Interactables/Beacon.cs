using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using Core;
using Noder.Nodes.Abstract;
using Settings;
using uwu.IO.SimpleFileBrowser.Scripts;
using uwu.Snippets;
using XNode.Examples.MathNodes;
using Neue;
using Interfaces;
using Objects.Base;
using Objects.Entities.Interactables.Empty;

public class Beacon: Interactable, IFlammable, ITooltip, IFileContainer {

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
    public static System.Action<Beacon> Activated, Deactivated, Destroyed, Deleted, Planted, Flowered;
    public static System.Action<Beacon> onUpdateState;

    // Properties

    [SerializeField] WorldPreset preset;
    [SerializeField] ParticleSystem deathPS, addPS, appearPS;
    
    new MeshRenderer renderer;
    new Collider collider;
    Material material;

    [SerializeField] GameObject pr_impactPS, pr_shinePS;
    [SerializeField] GameObject pr_flower;
    
    public Type type = Type.None;
    public Locale state = Locale.Terrain;
    public AGENT parent = AGENT.NULL;
    public Flower flower = null;
    
    [SerializeField] string m_file = null;

    public Vector3 origin = Vector3.zero;
    public Vector3 size = Vector3.one;

    // Attributes
    
    [SerializeField] bool hovered = false, m_discovered = false;
    [SerializeField] bool m_destroyed = false;

    [SerializeField] bool returnToOrigin = false;
    [SerializeField] Vector3 releasePosition = Vector3.zero, releaseScale = Vector3.zero;
    [SerializeField] float lerp_t = 0f, lerp_duration = 1f;
    AnimationCurve scaleCurve;
    
    
    #region Accessors

    public string File {
        get
        {
            return m_file;
        }
        set
        {
            m_file = value;
        }
    }

    public Vector3 Origin => origin;

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

    public bool destroyed
    {
        get { return m_destroyed; }
        private set { m_destroyed = value; }
    }

    public float knowledge 
    {
        get
        {
            return (Room == null)? 1f:Room.FetchBeaconKnowledgeMagnitude(this);
        }
    }
    
    public bool visible => state == Locale.Terrain;

    public ParticleSystem AppearPS => appearPS;

    #endregion

    #region Monobehaviour callbacks

    protected override void Update()
    {
        UpdateColor();
        
        base.Update();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (!returnToOrigin) return; // Ignore update calls
        
        lerp_t += Time.deltaTime;
        ReturnToOrigin(lerp_t);
    }

    void OnDisable() 
    {
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

	public void Register(Type type, Locale state, Vector3 origin, bool load = false)
    {
        Room = World.Instance;
        Nest = Nest.Instance;

        collider = GetComponent<Collider>();
        renderer = GetComponentInChildren<MeshRenderer>();
        material = renderer.material;

        this.type = type;
        var targetState = state;
        this.origin = origin;
        
        this.size = preset.normalBeaconScale * Vector3.one;
        this.lerp_duration = preset.beaconLerpDuration;
        this.scaleCurve = preset.beaconScaleCurve;

        ReturnToOrigin(99f);

        switch (targetState) 
        {
            case Locale.Flower:
                Flower(origin);
                break;
            case Locale.Nest:
                AddToNest();
                break;
            case Locale.Planted:
                Plant(origin);
                break;
            case Locale.Terrain:
                ToggleCapabilities(true);
                break;
            default: break;
        }
        this.state = state;

        if (OnRegister != null)
            OnRegister(this);
    }

    public void Unregister()
    {
        if (OnUnregister != null)
            OnUnregister(this);
    }

    #endregion

    #region Operations

    public bool AddToNest() 
    {
        if (state == Locale.Nest) return false;
        state = Locale.Nest;

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play(); // Trigger particle sys

        addPS.Play();
        deathPS.Stop();
        
        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        ToggleCapabilities(false);
        
        if (Activated != null)
            Activated(this);

        return true;
    }
    
    public bool RemoveFromNest(Vector3 origin, bool resetOrigin = false)
    {
        Debug.LogFormat("Deactivate {0} when state is {1}", File, state);
        if (state != Locale.Nest) return false;
        state = Locale.Terrain;

        deathPS.Play();
        addPS.Stop();

        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        transform.localScale = Vector3.zero;

        if (resetOrigin) this.origin = origin;
        ReturnToOrigin(-1f, initial:true);

        if (Deactivated != null)
            Deactivated(this);

        return true;
    }

    public bool Plant(Vector3 point)
    {
        if (state == Locale.Planted) return false;
        state = Locale.Planted;
        
        origin = point;
        ToggleCapabilities(false);

        if (Planted != null)
            Planted(this);

        return true;
    }

    public bool Flower(Vector3 point)
    {
        if (state == Locale.Flower) return false;
        state = Locale.Flower;
        
        origin = point;
        ToggleCapabilities(false);

        if (flower == null) 
        {
            var flowerInstance = Instantiate(pr_flower, origin, Quaternion.identity);
            
            flower = flowerInstance.GetComponentInChildren<Flower>(); 
            flower.Grow(Objects.Entities.Interactables.Empty.Flower.Origin.Beacon, File, type);    
        }

        if (Flowered != null)
            Flowered(this);
        
        return true;
    }

    public bool Delete(bool events = true)
    {
        if (state == Locale.Destroyed) return false;
        state = Locale.Destroyed;

        Unregister();

        if (Deleted != null && events) 
        {
            Deleted(this);
        }
        
        Destroy(gameObject);

        return true;
    }

    public void Grab()
    {
        if (state != Locale.Terrain) return;
        state = Locale.Drag;
    }

    public void Release()
    {
        if (state != Locale.Drag) return;
        state = Locale.Terrain;
        
        ReturnToOrigin(-1f, initial:true);
    }

    public bool Destroy()
    {
        if (state != Locale.Drag) return false;
        state = Locale.Terrain;
        
        ReturnToOrigin(-1f, initial:true);
        Fire();

        if (Destroyed != null) 
            Destroyed(this);

        return true;
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
    
    #region Capabilities

    void ToggleCapabilities(bool capable)
    {
        interactive = capable;
        collider.enabled = capable;

        transform.localScale = (capable) ? size : Vector3.zero;
    }
    
    #endregion
    
    #region Flammable

    public bool IsOnFire
    {
        get => deathPS.isPlaying;
    }
    
    public void Fire()
    {
        deathPS.Play();
    }

    public void Extinguish()
    {
        deathPS.Stop();
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

        if (complete) 
        {
            returnToOrigin = false; // Stop lerping if arrived
            ToggleCapabilities(true); // Re-enable capabilities
            
            deathPS.Stop();
        }
    }

	#endregion

    #region Info
    
    public string GetInfo()
    {
        return File;
    }
    
    #endregion
}
