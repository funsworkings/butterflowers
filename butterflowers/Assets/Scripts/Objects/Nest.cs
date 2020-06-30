using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

    #region External

    [SerializeField] Quilt Quilt = null;
    GameDataSaveSystem Save;

	#endregion

	#region Events

	public UnityEvent onOpen, onClose;
    public UnityEvent onIngestBeacon, onReleaseBeacon;

    public System.Action<Beacon> onAddBeacon, onRemoveBeacon;
    public System.Action<int> onUpdateCapacity;

	#endregion

	#region Properties

	ApplyGravityRelativeToCamera gravity_ext;
    Interactable interactable;
    Material mat;
    new Collider collider;
    new Rigidbody rigidbody;

    [SerializeField] ParticleSystem sparklesPS, cometPS;
    [SerializeField] GameObject pr_impactPS;

	#endregion

	#region Attributes

	public bool open = false, queue = false;

    [SerializeField] bool disposeOnClose = true;

    [Header("Physics")]
        [SerializeField] float force = 10f, m_energy = 0f;
        [SerializeField] float energyDecaySpeed = 1f, timeSinceEnergyBoost = 0f;

    [Header("Beacons")]
        [SerializeField] List<Beacon> m_beacons = new List<Beacon>();
        [SerializeField] int m_capacity = 12;

    [Header("Appearance")]
        [SerializeField] float colorSmoothSpeed = 1f;
        [SerializeField] Color inactiveColor, t_color;

	#endregion

	#region Accessors

    public int capacity { get { return m_capacity; } }
    public Beacon[] beacons { get { return m_beacons.ToArray(); } }
    public float energy => m_energy;

	#endregion

	#region Monobehaviour callbacks

	void Awake()
    {
        Instance = this;

        gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
        interactable = GetComponent<Interactable>();
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();

        mat = GetComponent<Renderer>().material;
    }

    void OnEnable()
    {
        Sun.onCycle += AttemptUpdateCapacity;
    }

    void OnDisable()
    {
        Sun.onCycle -= AttemptUpdateCapacity;
    }

    void Start()
    {
        Save = GameDataSaveSystem.Instance;

        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
        interactable.onGrab += Kick;

        Beacon.Destroyed += onDestroyBeacon;
    }

    void Update()
    {
        /*if (Input.GetMouseButtonDown(1) && queue) {
            var beacon = RemoveLastBeacon();
            bool success = beacon != null;
            if(success)
                Events.ReceiveEvent(beacon.file_abbreviated, "removed from NEST", Agent.Inhabitant0);
        }*/

        if (energy > 0f)
        {
            timeSinceEnergyBoost += Time.deltaTime;
            m_energy = Mathf.Max(0f, m_energy - Time.deltaTime * energyDecaySpeed * Mathf.Pow(timeSinceEnergyBoost, 2f));
        }

        UpdateColorFromState();
    }

    void OnDestroy()
    {
        interactable.onHover -= Hover;
        interactable.onUnhover -= Unhover;
        interactable.onGrab -= Kick;

        Beacon.Destroyed -= onDestroyBeacon;
    }

    #endregion

    #region Interactable callbacks

    void Hover(Vector3 origin, Vector3 normal)
    {
        queue = true;
    }

    void Unhover(Vector3 origin, Vector3 normal)
    {
        queue = false;
    }

    void Kick(Vector3 origin, Vector3 direction)
    {
        Vector3 dir = (-direction - 3f * gravity_ext.gravity).normalized;
        AddForceAndOpen(origin, dir, force, AGENT.Inhabitant0);
    }

    void AddForceAndOpen(Vector3 point, Vector3 direction, float force, AGENT agent = AGENT.Inhabitants)
    {
        rigidbody.AddForceAtPosition(direction * force, point);

        var impact = Instantiate(pr_impactPS, point, pr_impactPS.transform.rotation);
        impact.transform.up = direction.normalized;
        impact.GetComponent<ParticleSystem>().Play();

        Open();

        Events.ReceiveEvent(EVENTCODE.NESTKICK, agent, AGENT.Nest);
    }

    #endregion

    #region Operations

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

    public void RandomKick(AGENT agent = AGENT.Inhabitants)
    {
        Vector3 sphere_pos = Random.insideUnitSphere;
        Vector3 ray_origin = sphere_pos * 5f;
        Vector3 ray_dir = -sphere_pos;

        var ray = new Ray(transform.position + ray_origin, ray_dir);
        var hit = new RaycastHit();

        if (collider.Raycast(ray, out hit, 10f)) 
        {
            var normal = hit.normal;

            Vector3 origin = hit.point;
            Vector3 dir = (-normal - 3f * gravity_ext.gravity).normalized;

            AddForceAndOpen(origin, dir, force, agent);
        }
    }

    public void RestoreCapacity(int capacity)
    {
        m_capacity = capacity;
        Quilt.UpdateTextureCap(capacity);

        if (capacity > 6f)
            StartCoroutine("Scale");
    }

    #endregion

    #region Appearance

    void UpdateColorFromState()
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

	#region Beacon operations

	public bool AddBeacon(Beacon beacon)
    {
        if (m_beacons.Contains(beacon)) return false;

        var a = beacon.origin;
        var b = transform.position;

        beacon.WarpFromTo(true);

        m_beacons.Add(beacon);

        return true;
    }

    public bool RemoveBeacon(Beacon beacon)
    {
        if (!m_beacons.Contains(beacon)) return false;

        var a = transform.position;
        var b = beacon.origin;

        beacon.WarpFromTo(false);

        m_beacons.Remove(beacon);
        cometPS.Play();

        onReleaseBeacon.Invoke();
        if (onRemoveBeacon != null) onRemoveBeacon(beacon);

        return true;
    }

    public void ReceiveBeacon(Beacon beacon)
    {
        sparklesPS.Play();

        var dispose = (m_beacons.Count > capacity);
        if (dispose) 
        {
            Dispose(true);

            Events.ReceiveEvent(EVENTCODE.NESTSPILL, AGENT.Inhabitants, AGENT.Nest);

            return;
        }

        timeSinceEnergyBoost = 0f;
        m_energy = 1f;

        onIngestBeacon.Invoke();
        if (onAddBeacon != null) onAddBeacon(beacon);
    }

    public Beacon RemoveLastBeacon()
    {
        if (m_beacons == null || m_beacons.Count == 0) return null;

        var beacon = m_beacons[m_beacons.Count - 1];
        RemoveBeacon(beacon);

        return beacon;
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

    #region Sun callbacks

    void AttemptUpdateCapacity()
    {
        bool resize = false;

        int curr_size = beacons.Length;
        if (curr_size == capacity) 
        {
            Events.ReceiveEvent(EVENTCODE.NESTGROW, AGENT.Inhabitants, AGENT.Nest);

            m_capacity += 6;
            Quilt.UpdateTextureCap(capacity);

            resize = true;
        }
        else {
            Events.ReceiveEvent(EVENTCODE.NESTSHRINK, AGENT.Inhabitants, AGENT.Nest);

            if (capacity != 6)
                resize = true;

            m_capacity = 6;
            Quilt.UpdateTextureCap(capacity);

            Dispose();
        }

        if(resize)
            StartCoroutine("Scale");

        Save.nestcapacity = capacity;
        if (onUpdateCapacity != null)
            onUpdateCapacity(capacity);
    }

    IEnumerator Scale()
    {
        float t = 0f;
        float dur = .067f;

        Vector3 a_scale = transform.localScale;
        Vector3 t_scale = (1f + .167f*((capacity-6f) / 6f)) * Vector3.one;

        transform.localScale = t_scale * 2f;

        yield return new WaitForSeconds(.0033f);

        while (t <= dur) {
            t += Time.deltaTime;

            var interval = Mathf.Clamp01(t / dur);
            transform.localScale = Vector3.Lerp(a_scale, t_scale, Mathf.Pow(interval, 2f));

            yield return null;
        }
    }

	#endregion
}
