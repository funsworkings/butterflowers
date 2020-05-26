using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

    ApplyGravityRelativeToCamera gravity_ext;
    Interactable interactable;
    new Rigidbody rigidbody;

    public bool open = false;

    [Header("Physics")]
    [SerializeField] float force = 10f;

    [Header("Butterflies")]
    [SerializeField] MotherOfButterflies spawner = null;
    [SerializeField] List<Butterfly> butterflies = new List<Butterfly>();
    [SerializeField] bool killButterfliesOnSunCycle = true;

    [Header("Beacons")]
    [SerializeField] List<Beacon> beacons = new List<Beacon>();

    void Awake()
    {
        Instance = this;

        gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
        interactable = GetComponent<Interactable>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (killButterfliesOnSunCycle) Sun.onCycle += Close;

        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;

        interactable.onGrab += Kick;
    }

    void OnDestroy()
    {
        if (killButterfliesOnSunCycle) Sun.onCycle -= Close;

        Butterfly.OnRegister -= AddButterfly;
        Butterfly.OnUnregister -= RemoveButterfly;

        interactable.onGrab -= Kick;
    }

    #region Interactable callbacks

    void Kick(Vector3 origin, Vector3 direction)
    {
        Vector3 dir = (-direction - 3f * gravity_ext.gravity).normalized;

        rigidbody.AddForceAtPosition(dir * force, origin);
        Open();
    }

    void Follow(Vector3 origin, Vector3 direction)
    {
        rigidbody.AddForce(-(origin - transform.position).normalized * force);
    }

    #endregion

    #region Butterfly operations

    void AddButterfly(Butterfly butterfly)
    {
        butterflies.Add(butterfly);
    }

    void RemoveButterfly(Butterfly butterfly)
    {
        butterflies.Remove(butterfly);
    }

    void KillButterflies()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Kill();
    }

    void ReleaseButterflies()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Release();
    }

    void Open()
    {
        if (!open)
            ReleaseButterflies();

        open = true;
    }

    void Close()
    {
        if (open)
            KillButterflies();

        open = false;
    }

    #endregion

    #region Beacon operations

    public void IngestBeacon(Beacon beacon)
    {
        var position = transform.position;
        beacon.Warp(position);
    }

    public void ReleaseBeacon(Beacon beacon)
    {
        var position = beacon.origin;
        beacon.Warp(position);
    }

	#endregion
}
