using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

    #region Events

    public UnityEvent onOpen, onClose;
    public System.Action<Beacon> onAddBeacon, onRemoveBeacon;

	#endregion

	#region Properties

	ApplyGravityRelativeToCamera gravity_ext;
    Interactable interactable;
    new Rigidbody rigidbody;

	#endregion

	#region Attributes

	public bool open = false, queue = false;

    [Header("Physics")]
        [SerializeField] float force = 10f;

    [Header("Beacons")]
        [SerializeField] List<Beacon> beacons = new List<Beacon>();

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
        interactable.onHover += Hover;
        interactable.onUnhover += Unhover;
        interactable.onGrab += Kick;
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

    public void Dispose()
    {
        var beacons = this.beacons.ToArray();
        for (int i = 0; i < beacons.Length; i++) 
            RemoveBeacon(beacons[i]);

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
        if (onRemoveBeacon != null) onRemoveBeacon(beacon);
    }

    public void ReceiveBeacon(Beacon beacon)
    {
        if (onAddBeacon != null) onAddBeacon(beacon);
    }

    public void RemoveLastBeacon()
    {
        if (beacons == null || beacons.Count == 0) return;

        var beacon = beacons[beacons.Count - 1];
        RemoveBeacon(beacon);
    }

	#endregion
}
