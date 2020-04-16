using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    public static Nest Instance = null;

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

        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Sun.Instance.onCycle += Close;

        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;
    }

    void OnDestroy()
    {
        Sun.Instance.onCycle -= Close;

        Butterfly.OnRegister -= AddButterfly;
        Butterfly.OnUnregister -= RemoveButterfly;
    }

    void OnMouseDown()
    {
        var direction = (Random.insideUnitSphere + Vector3.up*5f).normalized;
        rigidbody.AddForce(force * direction);

        Open();
    }

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
