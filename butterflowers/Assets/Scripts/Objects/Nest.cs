using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

    #region External

    Quilt Quilt = null;

	#endregion

	#region Events

	public UnityEvent onOpen, onClose;
    public UnityEvent onIngestBeacon, onReleaseBeacon;

    public System.Action<Beacon> onAddBeacon, onRemoveBeacon;

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

    void Start()
    {
        Quilt = Quilt.Instance;

        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
        interactable.onGrab += Kick;

        Beacon.Destroyed += onDestroyBeacon;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && queue) RemoveLastBeacon();

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
        AddForceAndOpen(origin, dir, force);
    }

    void AddForceAndOpen(Vector3 point, Vector3 direction, float force)
    {
        rigidbody.AddForceAtPosition(direction * force, point);

        var impact = Instantiate(pr_impactPS, point, pr_impactPS.transform.rotation);
        impact.transform.up = direction.normalized;
        impact.GetComponent<ParticleSystem>().Play();

        Open();
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

    public void Dispose(bool release = true)
    {
        var beacons = this.m_beacons.ToArray();
        for (int i = 0; i < beacons.Length; i++) {
            if (release) RemoveBeacon(beacons[i]);
            else beacons[i].Delete();
        }

        this.m_beacons = new List<Beacon>();

        Quilt.Dispose(true);
    }

    public void RandomKick()
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

            AddForceAndOpen(origin, dir, force);
        }
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

	public void AddBeacon(Beacon beacon)
    {
        if (m_beacons.Contains(beacon)) return;

        var a = beacon.transform.position;
        var b = transform.position;

        beacon.WarpFromTo(a, b, true);

        m_beacons.Add(beacon);
    }

    public void RemoveBeacon(Beacon beacon)
    {
        if (!m_beacons.Contains(beacon)) return;

        var a = transform.position;
        var b = beacon.origin;

        beacon.WarpFromTo(a, b, false);

        m_beacons.Remove(beacon);
        cometPS.Play();

        onReleaseBeacon.Invoke();
        if (onRemoveBeacon != null) onRemoveBeacon(beacon);
    }

    public void ReceiveBeacon(Beacon beacon)
    {
        sparklesPS.Play();

        var dispose = (m_beacons.Count > capacity);
        if (dispose) 
        {
            Dispose(true);
            return;
        }

        timeSinceEnergyBoost = 0f;
        m_energy = 1f;

        onIngestBeacon.Invoke();
        if (onAddBeacon != null) onAddBeacon(beacon);
    }

    public void RemoveLastBeacon()
    {
        if (m_beacons == null || m_beacons.Count == 0) return;

        var beacon = m_beacons[m_beacons.Count - 1];
        RemoveBeacon(beacon);
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
}
