using System;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace butterflowersOS.Objects.Entities
{
    public class Cage : MonoBehaviour
    {
    
        // Events

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
    
        [SerializeField] Sector[] sectors = new Sector[]{};
        [SerializeField] Sprite[] shapes = new Sprite[] { };
        [SerializeField] bool complete = false;
        [SerializeField] bool hasCompletedAll = false;

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

            CheckIfComplete();
            hasCompletedAll = complete;
            
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

        /// <summary>
        /// Returns if vertex has already been activated by a different vine
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ActivateVertex(int index)
        {
            var sector = sectors[index];
            return sector.Complete();
        }

        /// <summary>
        /// Set aside a corner of the terrain for a vine's growth
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool QueueVertex(int index)
        {
            var sector = sectors[index];
            
            CheckIfAllQueued(out int sectorProgress, events:false);
            sectorProgress = Mathf.Min(sectorProgress, sectors.Length - 1);

            bool success = sector.Activate(shapes[UnityEngine.Random.Range(0, shapes.Length)]);//sector.Activate(shapes[sectorProgress]);

            CheckIfAllQueued(out sectorProgress);
            return success;
        }

        void CheckIfAllQueued(out int progress, bool events = true)
        {
            bool allQueued = false;
            
            progress = 0;
            if (!hasCompletedAll) 
            {
                foreach (Sector sector in sectors) 
                {
                    if (sector._Status != Sector.Status.Wait) 
                    {
                        ++progress;
                    }
                }

                allQueued = (progress == sectors.Length);
            }
            else 
            {
                progress = sectors.Length-1;
            }

            if (allQueued && events) // All corners have been queued/completed
            {
                onComplete.Invoke();
                hasCompletedAll = true;
            }
        }

        public void CheckIfComplete()
        {
            bool didComplete = true;
            
            foreach (Sector sector in sectors) 
            {
                if (sector._Status != Sector.Status.Active) 
                {
                    didComplete = false;
                    break;
                }
            }

            complete = didComplete;
        }
    
        #endregion
    }
}
