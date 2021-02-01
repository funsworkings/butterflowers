using System;
using UnityEngine;
using UnityEngine.Events;

namespace butterflowersOS.Objects.Entities
{
    public class Cage : MonoBehaviour
    {
    
        // Events

        public System.Action onUpdateActiveCorners;
        public System.Action onCompleteCorners;
        
        public UnityEvent onComplete;
    
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
        [SerializeField] bool complete = false;

        public bool load = false;
    
        #region Accessors

        public Sector[] Sectors => sectors;
        public bool Completed => complete;
    
        #endregion

        #region Initialization

        public void Initialize(sbyte[] statuses)
        {
            int index = 0;
            foreach (Sector sector in sectors)
                sector.Load(this, statuses[index++]);

            CheckIfComplete(events: false);
            if (complete) hasCompletedAll = true;
            
            load = true;
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
            CheckIfAllQueued();
        
            return true;
        }

        bool hasCompletedAll = false;

        void CheckIfAllQueued()
        {
            int numReady = 0;
            int numComplete = 0;
            
            foreach (Sector sector in sectors) 
            {
                if (sector._Status == Sector.Status.Queue) 
                {
                    numReady++;
                }
                if (sector._Status == Sector.Status.Active) 
                {
                    numComplete++;
                    numReady++;
                }
            }

            if (numReady == sectors.Length && !hasCompletedAll) // All corners have been queued/completed
            {
                onComplete.Invoke();
                hasCompletedAll = true;
            }
        }

        public void CheckIfComplete(bool events = true)
        {
            bool _complete = true;
            foreach (Sector sector in sectors) 
            {
                if (sector._Status != Sector.Status.Active) 
                {
                    _complete = false;
                    break;
                }
            }

            if (_complete && events) {

                if (onCompleteCorners != null)
                    onCompleteCorners();

            }

            complete = _complete;
        }
    
        #endregion
    }
}
