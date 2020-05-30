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
    new Rigidbody rigidbody;

    [SerializeField] ParticleSystem sparklesPS, cometPS;

	#endregion

	#region Attributes

	public bool open = false, queue = false;

    [Header("Physics")]
        [SerializeField] float force = 10f;

    [Header("Beacons")]
        [SerializeField] List<Beacon> beacons = new List<Beacon>();
        [SerializeField] int m_capacity = 12;

	#endregion

	#region Accessors

    public int capacity { get { return m_capacity; } }

	#endregion

	#region Monobehaviour callbacks

	void Awake()
    {
        Instance = this;

        gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
        interactable = GetComponent<Interactable>();
        rigidbody = GetComponent<Rigidbody>();
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

        rigidbody.AddForceAtPosition(dir * force, origin);
        Open();
    }

    #endregion

    #region Operations

    public void Open()
    {
        if (!open) {
            onOpen.Invoke();
        }
        open = true;
    }

    public void Close()
    {
        if (open) {
            onClose.Invoke();
        }
        open = false;
    }

    public void Dispose(bool release = true)
    {
        var beacons = this.beacons.ToArray();
        for (int i = 0; i < beacons.Length; i++) {
            if (release) RemoveBeacon(beacons[i]);
            else beacons[i].Delete();
        }

        this.beacons = new List<Beacon>();
    }

    #endregion

    #region Beacon operations

    public void AddBeacon(Beacon beacon)
    {
        if (beacons.Contains(beacon)) return;

        var a = beacon.transform.position;
        var b = transform.position;

        beacon.WarpFromTo(a, b, true);

        beacons.Add(beacon);
    }

    public void RemoveBeacon(Beacon beacon)
    {
        if (!beacons.Contains(beacon)) return;

        var a = transform.position;
        var b = beacon.origin;

        beacon.WarpFromTo(a, b, false);

        beacons.Remove(beacon);
        cometPS.Play();

        onReleaseBeacon.Invoke();
        if (onRemoveBeacon != null) onRemoveBeacon(beacon);
    }

    public void ReceiveBeacon(Beacon beacon)
    {
        sparklesPS.Play();

        var dispose = (beacons.Count > capacity);
        if (dispose) 
        {
            Dispose(true);
            return;
        }

        onIngestBeacon.Invoke();
        if (onAddBeacon != null) onAddBeacon(beacon);
    }

    public void RemoveLastBeacon()
    {
        if (beacons == null || beacons.Count == 0) return;

        var beacon = beacons[beacons.Count - 1];
        RemoveBeacon(beacon);
    }

    #endregion

    #region Beacon helpers

    public bool HasBeacon(Beacon beacon)
    {
        return beacons.Contains(beacon);
    }

    #endregion

    #region Beacon callbacks

    void onDestroyBeacon(Beacon beacon)
    {
        if (!beacons.Contains(beacon)) return;

        Quilt.Pop(beacon.file);

        beacons.Remove(beacon);
        cometPS.Play();
    }

	#endregion
}
