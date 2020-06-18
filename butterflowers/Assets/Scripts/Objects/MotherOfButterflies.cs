﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using Settings;

///<summary>Mother of all of the butterflies</summary>
public class MotherOfButterflies : Spawner
{
    public static MotherOfButterflies Instance = null;

    [SerializeField] 
    WorldPreset preset = null;

    #region External

    [SerializeField] new Camera camera;

    #endregion

    #region Attributes

    [SerializeField] float minDistance = 0f, maxDistance = 1f;
    [SerializeField] int alive = 0, dead = 0;

    public float minKillDistance => minDistance/2f;

    #endregion

    #region Collections

    List<Butterfly> butterflies = new List<Butterfly>();

	#endregion

	#region Monobehaviour callbacks

	protected override void Awake()
    {
        Instance = this;
        amount = preset.amountOfButterflies;

        base.Awake();
    }

    // Start is called before the first frame updaste
    protected override void Start()
    {
        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;

        Butterfly.Dying += ButterflyDying;
        Butterfly.Died += ResetButterfly;

        base.Start(); 
    }

    protected override void Update(){
        base.Update();
        amount = preset.amountOfButterflies;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Butterfly.OnRegister -= AddButterfly;
        Butterfly.OnUnregister -= RemoveButterfly;

        Butterfly.Dying -= ButterflyDying;
        Butterfly.Died -= ResetButterfly;
    }

    #endregion

    #region Spawner overrides

    protected override void DecidePosition(ref Vector3 pos)
    {
        if (camera != null)
        {
            Vector3 offset = Vector3.zero, position = Vector3.zero;

            offset = new Vector3(Random.Range(0f, 1f),
                                     Random.Range(0f, 1f),
                                     Random.Range(minDistance, maxDistance));

            position = camera.ViewportToWorldPoint(offset);
            pos = position;
        }
        else
            base.DecidePosition(ref pos);
    }

    void ResetButterfly(Butterfly butterfly)
    {
        InstantiatePrefab(butterfly.gameObject);
        butterfly.Reset();
    }

    protected override void SetPrefabAttributes(GameObject instance, Vector3 position, Quaternion rotation)
    {
        Butterfly butterfly = instance.GetComponent<Butterfly>();
        butterfly.position = position;

        butterfly.transform.localScale = Vector3.zero;
        butterfly.transform.rotation = rotation;
    }

    protected override void onInstantiatePrefab(GameObject obj, bool refresh)
    {
        base.onInstantiatePrefab(obj, refresh);

        alive++;
        if (refresh) dead--;
    }

    #endregion

    #region Butterfly operations

    public void KillButterflies()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Kill();
    }

    public void ReleaseButterflies()
    {
        foreach (Butterfly butterfly in butterflies)
            butterfly.Release();
    }

    #endregion

    #region Butterfly callbacks

    void AddButterfly(Butterfly butterfly)
    {
        butterflies.Add(butterfly);
    }

    void RemoveButterfly(Butterfly butterfly)
    {
        butterflies.Remove(butterfly);

        if (butterfly.state == Butterfly.State.Dying)
            dead--;
        else
            alive--;
    }

    void ButterflyDying(Butterfly butterfly)
    {
        ++dead;
        --alive;
    }

    #endregion

    #region Health operations

    public float GetHealth()
    {
        int total = (alive + dead);
        if (total == 0) return 1f;

        return ((float)alive / total);
    }

	#endregion
}
