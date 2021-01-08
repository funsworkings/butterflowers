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
using UnityEngine.Events;

public class Beacon: Interactable, IFlammable, ITooltip, IFileContainer {

    #region Internal

    public enum Type 
    {
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

    public enum Locale 
    {
        Terrain,
        Nest,
        Planted,
        Destroyed,
        Drag,
        Flower
    }

    [System.Serializable]
    public struct Transition
    {
        [System.Serializable] public class Event :UnityEvent<Beacon, Vector3> {}
        public Event onBegin, onEnd;

        [HideInInspector] public Vector3 posA, posB;
        [HideInInspector] public Vector3 scaleA, scaleB;

        public float time;
        public float duration;
        public float height;

        public AnimationCurve heightCurve;
        public AnimationCurve scaleCurve;
        public AnimationCurve positionCurve;

        public bool isValid;

        public bool Continue(Beacon beacon, float dt)
        {
            bool flagStart = (time <= 0f), flagEnd = (time+dt >= duration);

            time += dt;

            float interval = Mathf.Clamp01(time / duration);

            Vector3 position = (posA != posB)? Vector3.Lerp(posA, posB, positionCurve.Evaluate(interval)) : posB;
                    position += (Vector3.up * height * heightCurve.Evaluate(interval));

            float scaleLerp = scaleCurve.Evaluate(interval);
            Vector3 scale =  (scaleB - scaleA) * scaleLerp + scaleA;

            beacon.transform.position = position;
            beacon.transform.localScale = scale;
            
            if(flagStart) onBegin.Invoke(beacon, position);
            if(flagEnd) onEnd.Invoke(beacon, position);
            
            return time >= duration;
        }
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
    [SerializeField] ParticleSystem deathPS;
    [SerializeField] TrailRenderer trails;
    
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
    
    [SerializeField] Transition releaseTransition;

    // Attributes

    [SerializeField] Transition transition;
    [SerializeField] bool transitioning = false;
    
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

    public TrailRenderer Trails => trails;

    #endregion

    #region Monobehaviour callbacks

    protected override void Update()
    {
        UpdateColor();
        base.Update();

        if (transitioning) 
        {
            transitioning = !transition.Continue(this, Time.deltaTime);
            ToggleCapabilities(!transitioning);
        }
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

	public void Register(Type type, Locale state, Vector3 origin, Transition transition, bool load = false)
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

        if (load) 
        {
            switch (targetState) // Trigger initial state from target (normal mechanics)
            {
                case Locale.Flower:
                    Flower(origin, events: false);
                    break;
                case Locale.Nest:
                    AddToNest();
                    break;
                case Locale.Planted:
                    Plant(origin, events: false);
                    break;
                case Locale.Terrain:
                    ToggleCapabilities(true);
                    transform.localScale = size;
                    break;
                default: break;
            }
        }
        this.state = state;
        
        StartTransition(transition);
        if (transitioning) 
        {
            transitioning = !this.transition.Continue(this, Time.deltaTime);
            ToggleCapabilities(!transitioning);
        }

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

    public bool AddToNest(bool events = true) 
    {
        if (state == Locale.Nest) return false;
        state = Locale.Nest;

        var impact = Instantiate(pr_impactPS, transform.position, transform.rotation);
        impact.GetComponent<ParticleSystem>().Play(); // Trigger particle sys

        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        
        ToggleCapabilities(false);
        transform.localScale = Vector3.zero;
        
        if (Activated != null && events)
            Activated(this);

        return true;
    }
    
    public bool RemoveFromNest(Vector3 origin, bool resetOrigin = false, bool events = true)
    {
        Debug.LogFormat("Deactivate {0} when state is {1}", File, state);
        if (state != Locale.Nest) return false;
        state = Locale.Terrain;

        transform.position = Nest.transform.position; // Reset back to nest position at start of lerp
        transform.localScale = Vector3.zero;

        if (resetOrigin) this.origin = origin;
        ReleaseTransition();

        if (Deactivated != null && events)
            Deactivated(this);

        return true;
    }

    public bool Plant(Vector3 point, bool events = true)
    {
        if (state == Locale.Planted) return false;
        state = Locale.Planted;
        
        origin = point;
        
        ToggleCapabilities(false);
        Extinguish();
        transform.localScale = Vector3.zero;

        if (Planted != null && events)
            Planted(this);

        return true;
    }

    public bool Flower(Vector3 point, bool events = true)
    {
        if (state == Locale.Flower) return false;
        state = Locale.Flower;
        
        origin = point;
        
        ToggleCapabilities(false);
        Extinguish();
        transform.localScale = Vector3.zero;

        if (flower == null) 
        {
            var flowerInstance = Instantiate(pr_flower, origin, Quaternion.identity);
            
            flower = flowerInstance.GetComponentInChildren<Flower>(); 
            flower.Grow(Objects.Entities.Interactables.Empty.Flower.Origin.Beacon, File, type);    
        }

        if (Flowered != null && events)
            Flowered(this);
        
        return true;
    }

    public bool Delete(bool events = true)
    {
        Debug.LogFormat("Attempt to delete beacon => {0}", m_file);
        
        if (state == Locale.Destroyed) return false;
        state = Locale.Destroyed;

        if (Deleted != null && events) 
        {
            Deleted(this);
        }
        
        Unregister();
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
        
        ReleaseTransition();
    }

    public bool Destroy(bool events = true)
    {
        if (state != Locale.Drag) return false;
        state = Locale.Terrain;
        
        ReleaseTransition();
        Fire();

        if (Destroyed != null && events) 
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

    #region Transitions

    public void StartTransition(Transition transition)
    {
        var _transition = transition;
        _transition.time = 0f;
        
        this.transition = _transition;
        if (_transition.isValid) transitioning = true;
    }

    void ReleaseTransition()
    {
        Transition _transition = releaseTransition;
            _transition.posA = transform.position;
            _transition.posB = origin;
            _transition.scaleA = transform.localScale;
            _transition.scaleB = size;

        StartTransition(_transition);
    }

	#endregion

    #region Info
    
    public string GetInfo()
    {
        return File.AppendActionableInformation(this);
    }
    
    #endregion
}
