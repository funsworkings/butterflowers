﻿using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;
using uwu;

using Vertex = Cage.Vertex;

public class VineManager : MonoBehaviour
{
    #region External

    GameDataSaveSystem Save;
    
    [SerializeField] BeaconManager Beacons;

    #endregion
    
    // Collections

    public List<Vine> vines = new List<Vine>();

    // Properties

    [SerializeField] WorldPreset Preset;

    [SerializeField] Cage cage;

    [SerializeField] GameObject vinePrefab = null;
    [SerializeField] Transform vineRoot = null;

    bool load = false;

	#region Monobehaviour callbacks

	void OnEnable()
    {
        Beacons.onPlantBeacon += onPlantBeacon;

        Vine.onCompleteNaturalGrowth += onVineCompleteNaturalGrowth;
        Vine.onCompleteGateGrowth += onVineCompleteGateGrowth;

        cage.onUpdateActiveCorners += onCageUpdatedActiveCorner;
    }

    void OnDisable()
    {
        Beacons.onPlantBeacon -= onPlantBeacon;
        
        Vine.onCompleteNaturalGrowth -= onVineCompleteNaturalGrowth;
        Vine.onCompleteGateGrowth -= onVineCompleteGateGrowth;
        
        cage.onUpdateActiveCorners -= onCageUpdatedActiveCorner;
    }

    void Update()
    {
        if (!load) return;
        
        if(vines.Count > 0)
            Save.vines = vines.ToArray();
    }

    #endregion

    #region Initialization

    public void Load(VineData[] dat)
    {
        Save = GameDataSaveSystem.Instance;
        
        cage.Initialize(Save.data.corners);
        
        if (dat != null) {
            foreach (VineData v in dat) {
                var vine = DropVine(transform.position, vineRoot.up);
                    vine.Initialize(this, cage, v);
                    
                vines.Add(vine);
            }
        }

        load = true;
    }

	#endregion

	#region Beacon manager callbacks

	void onPlantBeacon(Beacon beacon)
    {
        var origin = beacon.origin;
        
        var vine = DropVine(origin, vineRoot.up);
            vine.Initialize(this, cage,null);
        
        var file = beacon.file;
            vine.file = file;
            
        vines.Add(vine);
    }

    #endregion

    #region Vine operations

    Vine DropVine(Vector3 position, Vector3 normal)
    {
        var instance = Instantiate(vinePrefab, vineRoot);
            instance.transform.position = position;
            instance.transform.up = normal;

        var vine = instance.GetComponent<Vine>();
        return vine;
    }

    public float CalculateVineGrowSpeed(Vine vine)
    {
        float secondsToGrow = Preset.ConvertDaysToSeconds(Preset.daysToGrowVine);

        float distanceToTravel = vine.length;
        float speed = (distanceToTravel / secondsToGrow); // per second
        
        return speed;
    }

    #endregion
    
    #region Vine callbacks

    void onVineCompleteNaturalGrowth(Vine vine)
    {
        int vertexInd = 0;
        Vertex vertex = cage.GetClosestVertex(vine.end, out vertexInd);

        bool success = cage.ActivateVertex(vertexInd);
        if (success) 
        {
            vine.TransitionToGate(); // Turn into gate    
        }
        else
            vine.GrowFlower();
    }

    void onVineCompleteGateGrowth(Vine vine)
    {
        int vertexInd = 0;
        Vertex vertex = cage.GetClosestVertex(vine.end, out vertexInd);

        if (vertex.active) 
        {
            vine.GrowFlower();
        }
    }
    
    #endregion
    
    #region Cage callbacks

    void onCageUpdatedActiveCorner()
    {
        var corners = cage.active;
        Save.data.corners = corners;
    }
    
    #endregion
}
