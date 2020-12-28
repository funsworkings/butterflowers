﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Objects.Entities;
using UnityEditor;
using UnityEngine;
using XNode.Examples.MathNodes;

public class Cage : MonoBehaviour
{
    
    // Events

    public System.Action onUpdateActiveCorners;
    public System.Action onCompleteCorners;
    
    #region Internal

    [Serializable]
    public struct Vertex
    {
        public Vector3 top;
        public Vector3 bottom;
    }

    #endregion
    
    // Properties
    
    [SerializeField] Sector[] sectors;
    
    #region Accessors

    public Sector[] Sectors => sectors;
    
    #endregion

    #region Initialization

    public void Initialize(int[] statuses)
    {
        int index = 0;
        foreach (Sector sector in sectors)
            sector.Load(this, statuses[index++]);

        CheckIfComplete(events: false);
    }
    
    #endregion
    
    #region Bounds and vertices

    public Vertex GetClosestVertex(Vector3 point, out int index)
    {
        Vertex vertex = new Vertex();

        float bestDistance = Vector3.Distance(point, sectors[0].top);
        Sector bestSector = sectors[0];
        
        index = 0;

        for (int i = 1; i < sectors.Length; i++) 
        {
            float dist = Vector3.Distance(point, sectors[i].top);
            if (dist <= bestDistance) 
            {
                bestDistance = dist;
                bestSector = sectors[i];
                index = i;
            }
        }

        vertex.top = bestSector.top;
        vertex.bottom = bestSector.bottom;

        return vertex;
    }

    #endregion
    
    #region Activation

    public bool ActivateVertex(int index)
    {
        var sector = sectors[index];
        if (sector._Status != Sector.Status.Queue) return false;
        
       sector.Complete();
       if (onUpdateActiveCorners != null)
            onUpdateActiveCorners();

        CheckIfComplete(); // Check if cage was written during runtime
        
        return true;
    }

    public bool QueueVertex(int index)
    {
        var sector = sectors[index];
        if (sector._Status != Sector.Status.Wait) return false;
        
        sector.Activate();
        
        return true;
    }

    bool CheckIfComplete(bool events = true)
    {
        bool complete = true;
        foreach (Sector sector in sectors) 
        {
            if (sector._Status != Sector.Status.Active) 
            {
                complete = false;
                break;
            }
        }

        if (complete && events) {

            if (onCompleteCorners != null)
                onCompleteCorners();

        }

        return complete;
    }
    
    #endregion
}
