using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

    ApplyGravityRelativeToCamera gravity_ext;
    Interactable interactable;
    new Rigidbody rigidbody;

    [Header("Physics")]
    [SerializeField] float force = 10f;

    [Header("Butterflies")]
    [SerializeField] ButterflySpawner spawner = null;
    [SerializeField] List<Butterfly> butterflies = new List<Butterfly>();

    public bool open = false;

    void Awake()
    {
        Instance = this;

        gravity_ext = GetComponent<ApplyGravityRelativeToCamera>();
        interactable = GetComponent<Interactable>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Sun.Instance.onCycle += Close;

        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;

        interactable.onGrab += Kick;
    }

    void OnDestroy()
    {
        Sun.Instance.onCycle -= Close;

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

    void KillAll()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Kill();
    }

    void ReleaseAll()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Release();
    }

    void Open()
    {
        if (!open)
            ReleaseAll();

        open = true;
    }

    void Close()
    {
        if (open)
            KillAll();

        open = false;
    }

    #endregion
}
