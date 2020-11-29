using System;
using Settings;
using System.Collections;
using System.Collections.Generic;
using AI;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using uwu;
using uwu.Extensions;
using uwu.Snippets;
using Behaviour = AI.Types.Behaviour;
using Random = UnityEngine.Random;

public class Nest : Focusable, IReactToSunCycle
{
    public static Nest Instance = null;

    #region Events

    public UnityEvent onOpen, onClose;
    public UnityEvent onIngestBeacon, onReleaseBeacon;

    public System.Action<Beacon> onAddBeacon, onRemoveBeacon;
    public System.Action<int> onUpdateCapacity;

    #endregion

    #region External

    [SerializeField] Quilt Quilt = null;
    GameDataSaveSystem Save;
    Focusing Focusing;
    Camera main_camera;

	#endregion

	#region Properties

	ApplyGravityRelativeToCamera gravity_ext;
    uwu.Gameplay.Interactable interactable;
    Material mat;
    new Collider collider;
    new Rigidbody rigidbody;
    Damage damage;

    [SerializeField] ParticleSystem sparklesPS, cometPS;
    [SerializeField] GameObject pr_impactPS, pr_shinePS;

    [SerializeField] TMPro.TMP_Text infoText;

    #endregion

    #region Attributes

    [Header("General")]

    public bool open = false;
    public bool queue = false;

    [SerializeField] bool disposeOnClose = true;
    [SerializeField] WorldPreset worldPreset;

    [Header("Physics")]
        [SerializeField] float force = 10f, m_energy = 0f, m_globalEnergy = 0f;
        [SerializeField] float energyDecaySpeed = 1f, energyDecayDelay = 0f, timeSinceEnergyBoost = 0f;

    [Header("Beacons")]
        [SerializeField] List<Beacon> m_beacons = new List<Beacon>();
        [SerializeField] int m_capacity = 12;

    [Header("Appearance")]
        [SerializeField] float colorSmoothSpeed = 1f;
        [SerializeField] Color inactiveColor, t_color;

	#endregion

	#region Accessors

    public float fill {
        get
        {
            int cap = capacity;
            int amt = beacons.Length;

            return (1f * amt) / cap;
        }
    }

    public int capacity { get { return m_capacity; } }
    public Beacon[] beacons { get { return m_beacons.ToArray(); } }

    public float energy => m_energy;

    public Vector3 trajectory => rigidbody.velocity.normalized;

    public int LEVEL => Mathf.FloorToInt(capacity / 6f) - 1;

	#endregion

	#region Monobehaviour callbacks

	protected override void Awake()
    {
        base.Awake();

        Instance = this;

        gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
        interactable = GetComponent<uwu.Gameplay.Interactable>();
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
        damage = GetComponent<Damage>();

        mat = GetComponent<Renderer>().material;
    }

    protected override void OnStart()
    {
        base.OnStart();

        Save = GameDataSaveSystem.Instance;
        Focusing = FindObjectOfType<Focusing>();
        main_camera = Camera.main;

        m_capacity = worldPreset.nestCapacity;

        if (Quilt == null)
            Quilt = FindObjectOfType<Quilt>();

        damage.onHit.AddListener(SpillKick);

        Beacon.Destroyed += onDestroyBeacon;

        StartCoroutine("MaintainOnScreen");
    }

    protected override void Update()
    {
        base.Update();

        if (energy > 0f)
        {
            timeSinceEnergyBoost += Time.deltaTime;

            float t = Mathf.Max(0f, timeSinceEnergyBoost - energyDecayDelay);
            m_energy = Mathf.Max(0f, m_energy - Time.deltaTime * energyDecaySpeed * Mathf.Pow(t, 2f));
        }

        if (disposeduringframe) {
            DisposeDuringFrame();
            disposeduringframe = false;
        }

        UpdateColorFromStateAndCapacity();
        UpdateInfoTextFromCapacity();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        damage.onHit.RemoveListener(SpillKick);

        Beacon.Destroyed -= onDestroyBeacon;

        StopCoroutine("MaintainOnScreen");
    }

    #endregion

    #region Interactable callbacks

    protected override void onHover(Vector3 origin, Vector3 normal)
    {
        queue = true;

        base.onHover(origin, normal);
    }

    protected override void onUnhover(Vector3 origin, Vector3 normal)
    {
        queue = false;

        base.onUnhover(origin, normal);
    }

    protected override void onGrab(Vector3 origin, Vector3 direction)
    {
        Vector3 dir = (-direction - 3f * gravity_ext.gravity).normalized;
        AddForceAndOpen(origin, dir, force);
    }

    void AddForceAndOpen(Vector3 point, Vector3 direction, float force, AGENT agent = AGENT.User, bool particles = true, bool events = true)
    {
        rigidbody.AddForceAtPosition(direction * force, point);

        if (particles) 
        {
            var impact = Instantiate(pr_impactPS, point, pr_impactPS.transform.rotation);
                impact.transform.up = direction.normalized;
                impact.GetComponent<ParticleSystem>().Play();
        }

        Open();

        if(events) 
            Events.ReceiveEvent(EVENTCODE.NESTKICK, agent, AGENT.Nest);
    }

    #endregion

    #region Core operations

    public bool Open()
    {
        if (!open) {
            open = true;
            onOpen.Invoke();

            return true;
        }
        return false;
    }

    public bool Close()
    {
        if (open) {
            if (disposeOnClose) Dispose();

            open = false;
            onClose.Invoke();

            return true;
        }
        return false;
    }

    public bool Dispose(bool release = true)
    {
        bool success = this.m_beacons.Count > 0;

        var beacons = this.m_beacons.ToArray();
        for (int i = 0; i < beacons.Length; i++) {
            if (release) RemoveBeacon(beacons[i]);
            else beacons[i].Delete();
        }

        this.m_beacons = new List<Beacon>();

        Quilt.Dispose(true);

        return success;
    }

    public void Cycle(bool refresh)
    {
        Pulse();
        
        if (refresh) 
            Close();
    }

    #endregion

    #region Kicking

    void SpillKick()
    {
        RandomKick(1f, AGENT.NULL, ps: false, events: false);
    }

    public void RandomKick(float force = 1f, AGENT agent = AGENT.Inhabitants, bool ps = true, bool events = true)
    {
        Vector3 sphere_pos = Random.insideUnitSphere;
        Vector3 dir = -sphere_pos;

        Kick(dir, force, agent:agent);
    }

    public void Kick(Vector3 direction, float force = 1f, AGENT agent = AGENT.Inhabitants, bool ps = true, bool events = true)
    {
        Vector3 ray_origin = -direction * 5f;
        Vector3 ray_dir = direction;

        var ray = new Ray(transform.position + ray_origin, ray_dir);
        var hit = new RaycastHit();

        if (collider.Raycast(ray, out hit, 10f)) {
            var normal = hit.normal;

            Vector3 origin = hit.point;
            Vector3 dir = (-normal - 3f * gravity_ext.gravity).normalized;

            AddForceAndOpen(origin, dir, this.force * force, agent, particles: ps, events: events);
        }
    }

    IEnumerator MaintainOnScreen()
    {
        while (true) {
            yield return new WaitForSeconds(3f);

            if (!Focusing.active && open)  // Ignore if focus is focused on somethings
            {
                Vector2 screen_pos = Vector2.zero;

                bool visible = Extensions.IsVisible(transform, main_camera, out screen_pos);
                if (!visible) {
                    Vector3 target_pos = main_camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 10f));
                    Vector3 dir = (target_pos - transform.position).normalized;

                    Kick(dir, 1f, AGENT.World); // Kick nest towards screen pos
                }
            }
        }
    }

    #endregion

    #region Appearance

    void UpdateColorFromStateAndCapacity()
    {
        if (open) 
        {
            float fill = (float)beacons.Length / capacity;
            t_color = new Color(1f, (1f - fill), 1f);
        }
        else
            t_color = inactiveColor;

        mat.color = Color.Lerp(mat.color, t_color, Time.deltaTime * colorSmoothSpeed);
    }

    #endregion

    #region Info

    void UpdateInfoTextFromCapacity()
    {
        if (queue && infoText != null) {
            string message = "{0}\n\n{1}";

            string capacity = string.Format("{0} / {1}", beacons.Length, this.capacity);
            
            message = string.Format(message, capacity, defaultTooltipText);
            UpdateTooltipText(message);
        }
    }

	#endregion

	#region Beacon operations

	public bool AddBeacon(Beacon beacon)
    {
        if (m_beacons.Contains(beacon)) return false;
        m_beacons.Add(beacon);

        return true;
    }

    public bool RemoveBeacon(Beacon beacon)
    {
        if (!m_beacons.Contains(beacon)) return false;
        m_beacons.Remove(beacon);

        Debug.LogFormat("Nest REMOVE = {0}", beacon.file);
        beacon.Deactivate();

        cometPS.Play();

        onReleaseBeacon.Invoke();

        if (onRemoveBeacon != null) 
            onRemoveBeacon(beacon);

        return true;
    }

    public void ReceiveBeacon(Beacon beacon)
    {
        Debug.LogFormat("Nest ADD = {0}", beacon.file);

        sparklesPS.Play();

        var dispose = (m_beacons.Count > capacity);
        if (dispose) 
        {
            disposeduringframe = true;
            return;
        }
        else {
            disposeduringframe = false;
        }

        Pulse();

        onIngestBeacon.Invoke();
        if (onAddBeacon != null) onAddBeacon(beacon);
    }

    bool disposeduringframe = false;
    void DisposeDuringFrame()
    {
        Dispose(true);

        Events.ReceiveEvent(EVENTCODE.NESTSPILL, AGENT.User, AGENT.Nest);
        
        if(damage != null)
            damage.Hit();
    }

    public Beacon RemoveLastBeacon()
    {
        if (m_beacons == null || m_beacons.Count == 0) return null;

        var beacon = m_beacons[m_beacons.Count - 1];
        RemoveBeacon(beacon);

        return beacon;
    }

    public void Pulse()
    {
        timeSinceEnergyBoost = 0f;
        m_energy = 1f;
    }

    #endregion

    #region Beacon helpers

    public bool HasBeacon(Beacon beacon)
    {
        return m_beacons.Contains(beacon);
    }

    #endregion

    #region Beacon callbacks

    void onDestroyBeacon(Beacon beacon)
    {
        if (!m_beacons.Contains(beacon)) return;

        if(beacon.type != Beacon.Type.None) Quilt.Pop(beacon.file);

        m_beacons.Remove(beacon);
        cometPS.Play();

        onReleaseBeacon.Invoke();
    }

    #endregion
    
    #region Collisions

    void OnTriggerEnter(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity == null) return;

        if (entity is Vine) 
        {
            var vine = (entity as Vine);
            
            var file = vine.file;
            if(!string.IsNullOrEmpty(file)) 
            {
                vine.Flutter();
                Quilt.PushOverrideTexture(file);
                
                Pulse();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity == null) return;
        
        if (entity is Vine) 
        {
            var vine = (entity as Vine);
            
            var file = vine.file;
            if(!string.IsNullOrEmpty(file)) 
            {
                vine.Unflutter();
                Quilt.PopOverrideTexture(file);
            }
        }
    }

    #endregion
}
