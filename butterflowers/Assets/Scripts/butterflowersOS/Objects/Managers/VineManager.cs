﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Data;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Presets;
using UnityEngine;
using uwu;
using Vertex = butterflowersOS.Objects.Entities.Cage.Vertex;

namespace butterflowersOS.Objects.Managers
{
    public class VineManager : MonoBehaviour
    {
        #region External

        GameDataSaveSystem _Save;
        [SerializeField] BeaconManager Beacons;

        #endregion
    
        // Events

        public System.Action onUpdateVines;
        public System.Action<Vertex> onCompleteCorner;
    
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

        #endregion

        #region Save/load

        public System.Object Save()
        {
            VineSceneData dat = new VineSceneData();
        
            var vineDatas = new List<VineData>();
            for (var i = 0; i < vines.Count; i++) 
            {
                var vine = vines[i];
                var file = Library.Instance.FetchFileIndex(vine.File);
                
                var parsed = new VineData(vine.state, (byte)vine.index, (byte)Mathf.FloorToInt(vine.interval * 255f), vine.Waypoints, (ushort)file, vine.Leaves);

                vineDatas.Add(parsed);
            }

            dat.vines = vineDatas.ToArray();
            dat.corners = cage.Sectors.Select(corner => (sbyte)corner._Status).ToArray();
        
            return dat;
        }

        public void Load(System.Object dat)
        {
            var data = (VineSceneData) dat;
            StartCoroutine(ReliableLoad(data));
        }

        IEnumerator ReliableLoad(VineSceneData data)
        {
            cage.Initialize(data.corners);
            while (!cage.load) yield return null;
        
            yield return new WaitForEndOfFrame();
        
            foreach (VineData v in data.vines) 
            {
                string file = null;
                bool success = Library.Instance.FetchFile(v.file, out file);

                if (success) 
                {
                    var vine = DropVine(transform.position, vineRoot.up);
                    vine.Initialize(this, cage, file, v);

                    vines.Add(vine);
                }
            }

            _Save = GameDataSaveSystem.Instance;
            load = true;
        }

        #endregion

        #region Beacon manager callbacks

        void onPlantBeacon(Beacon beacon)
        {
            var origin = beacon.origin;
        
            var vine = DropVine(origin, vineRoot.up);
            vine.Initialize(this, cage,null);
        
            var file = beacon.File;
            vine.File = file;
            
            vines.Add(vine);

            if (onUpdateVines != null) onUpdateVines();
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

        public float SecondsToGrowVine()
        {
            return Preset.ConvertDaysToSeconds(Preset.daysToGrowVine);
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
            {
                vine.GrowFlower();
            }
        }

        void onVineCompleteGateGrowth(Vine vine)
        {
            vine.GrowFlower();

            if (onCompleteCorner != null)
                onCompleteCorner(vine.Vertex);
        }
    
        #endregion
    
        #region Cage callbacks

        void onCageUpdatedActiveCorner()
        {
            if (onUpdateVines != null) onUpdateVines();
        }
    
        #endregion
    }
}
