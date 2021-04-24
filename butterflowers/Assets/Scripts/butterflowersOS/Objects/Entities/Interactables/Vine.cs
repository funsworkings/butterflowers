using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Data;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Base;
using butterflowersOS.Objects.Entities.Interactables.Empty;
using butterflowersOS.Objects.Managers;
using butterflowersOS.Objects.Miscellaneous;
using butterflowersOS.Presets;
using UnityEngine;
using uwu.Extensions;

namespace butterflowersOS.Objects.Entities.Interactables
{
    public class Vine : Entity, ITooltip, IFileContainer
    {
        #region Internal

        public enum Status 
        {
            Natural,
            Gate,
            Complete
        }

        #endregion
    
        // Events

        public static System.Action<Vine> onCompleteNaturalGrowth, onCompleteGateGrowth;

        #region Properties

        [SerializeField] WorldPreset preset = null;

        LineRenderer line;
        new CapsuleCollider collider;
        VineManager Manager;

        [SerializeField] ParticleSystem smokePS = null;
        [SerializeField] GameObject wallPrefab = null;
        [SerializeField] GameObject flowerPrefab = null;
        [SerializeField] GameObject leafPrefab = null;

        #endregion

        #region Collections

        [SerializeField] List<Vector3> waypoints = new List<Vector3>();
        [SerializeField] List<Vector3> gate = new List<Vector3>();
    
        [SerializeField] List<Vector3> vertices = new List<Vector3>();
        [SerializeField] List<Leaf> leaves = new List<Leaf>();

        #endregion

        #region Attributes

        public Status state = Status.Natural;
        [SerializeField] string m_file = null;
    
        [SerializeField] float growRadius = 1f;
        [SerializeField] float snapDistance = .01f;
        [SerializeField] float growHeight = 4f;
        [SerializeField] float gateSpeed = 1f;
        [SerializeField] float saplingSpeed = 1f;
        [SerializeField] int minSegments = 3, maxSegments = 10;

        Cage.Vertex vertex;

        public float interval = 0f;
        public int index = -1;

        [SerializeField] List<int> gateIndices = new List<int>(); // 0 == TOP, 1 == TOP CORNER, 2 == FINAL POS
        [SerializeField] List<Vector3> gateTransition = new List<Vector3>();
    
        bool travelInProgress = false;
        bool initialized = false;
        bool complete = false;

        public bool refreshLeaves = false;
    
        #endregion

        #region Accessors

        public Vector3[] Waypoints => waypoints.ToArray();
        public Leaf[] Leaves => leaves.ToArray();

        public Vector3 end => line.GetPosition(line.positionCount - 1);

        public Cage.Vertex Vertex => vertex;

        public string File
        {
            get { return m_file; }
            set
            {
                m_file = value;
            }
        }

        public float length => waypoints.DistanceBetweenVectors();
        public float height => growHeight;

        [SerializeField] float m_growSpeed = 0f;

        public float growSpeed
        {
            get
            {
                m_growSpeed = Manager.CalculateVineGrowSpeed(this);
                return m_growSpeed;
            }
        }

        #endregion

        #region Monobehaviour callbacks

        protected void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 0;
            
            collider = GetComponent<CapsuleCollider>();
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!initialized) return; // Ignore if not initialized
        
            collider.enabled = (state == Status.Natural); // Disable collider if gated
            line.widthMultiplier = preset.vineWidth;

            if (travelInProgress) 
            {
                if (state == Status.Natural) 
                {
                    bool completed = NaturalGrowthUpdate();
                    UpdateColliderBounds();

                    if (completed) 
                    {
                        travelInProgress = false;  // Stop traveling when arrived
                        if (onCompleteNaturalGrowth != null)
                            onCompleteNaturalGrowth(this);
                    }
                
                }
                else // Gated ( community ) 
                {
                    bool completed = GatedGrowthUpdate();
                    if (completed) 
                    {
                        travelInProgress = false; // Stop traveling when arrived
                        if (onCompleteGateGrowth != null)
                            onCompleteGateGrowth(this);
                    }
                }
            }

            line.positionCount = vertices.Count;
            line.SetPositions(vertices.ToArray());
        }

        #endregion
    
        #region Entity overrides

        protected override bool EvaluateActiveState()
        {
            return (Sun.active || state == Status.Gate);
        }

        #endregion

        #region Growth
    
        public void Initialize(VineManager manager, Cage cage, string file, VineData data = null)
        {
            Manager = manager;

            bool refresh = (data != null);
        
            int vertexIndex = 0;
        
            if (data == null) 
            {
                vertex = cage.GetClosestVertex(transform.position, out vertexIndex);
            
                bool successQueue = cage.QueueVertex(vertexIndex);
                if (successQueue) 
                    growHeight = preset.maximumVineGrowHeight;
                else {
                    float min_h = preset.minimumVineGrowHeight;
                    float max_h = preset.maximumVineGrowHeight;

                    float spread = (max_h - min_h);
                    float vh = preset.vineHeightAllowance * spread;
                    
                    float depth = Mathf.Clamp(file.Split('/').Length, preset.minimumFileDepth, preset.maximumFileDepth) / (1f * preset.maximumFileDepth); // 0-1 depth value

                    float height = min_h + (depth * spread);
                    float offset = Random.Range(-vh, vh);
                    
                    growHeight = Mathf.Clamp(height + offset, min_h, max_h);
                }

                waypoints = new List<Vector3>(ConstructNaturalLine());
                leaves = new List<Leaf>(ConstructAllLeaves());

                state = Status.Natural; // Default state
                index = 0;
                interval = 1f;
            }
            else 
            {
                var len = data.waypoints_x.Length; // Get length of all waypoints
            
                waypoints = new List<Vector3>();
                for (int i = 0; i < len; i++) 
                    waypoints.Add(new Vector3(data.waypoints_x[i], data.waypoints_y[i], data.waypoints_z[i]) * Constants.VineWaypointSnapDistance);
            
                state = (Status) data.status; // Assign status
                index = data.index;
                interval = data.interval / 255f;
                File = file;

                transform.position = waypoints[0];
                transform.up = transform.parent.up;

                foreach (LeafData ld in data.leaves.leaves) 
                {
                    var leaf = ConstructLeaf(ld.index, ld.interval, offset:(ld.rotation / 255f) * 360f);
                    leaves.Add(leaf);
                }
                ParseAllLeaves(index, interval, true);
            
                vertex = cage.GetClosestVertex(waypoints.Last(), out vertexIndex);
                cage.QueueVertex(vertexIndex);
            }

            gate = new List<Vector3>(ConstructGatedLine(vertex)); // Construct gate from waypoints

            bool flagComplete = false;
        
            if (state == Status.Natural) 
            {
                if (interval > 0f)
                    travelInProgress = true;
                else
                    travelInProgress = false;

                int completedVertices = (index + 1);
                flagComplete = (completedVertices == waypoints.Count);

                vertices = new List<Vector3>(new Vector3[completedVertices]); 
                for (int i = 0; i < completedVertices; i++) {
                    if (i < completedVertices - 1 || i == 0) 
                    {
                        vertices[i] = waypoints[i];
                    }
                    else {
                        var a = waypoints[i - 1];
                        var b = waypoints[i];

                        vertices[i] = Vector3.Lerp(a, b, interval); // Move last vertex to interval position
                    }
                }
            }
            else 
            {
                vertices = new List<Vector3>(waypoints.ToArray());
                for (int i = 0; i < vertices.Count; i++) 
                {
                    vertices[i] = gate[i];
                }

                travelInProgress = false;
                flagComplete = true;
            }
        
            // Update all vertices in line on LOAD
            line.positionCount = vertices.Count;
            line.SetPositions(vertices.ToArray());

            complete = flagComplete;
            if (flagComplete) 
            {
                GrowFlower();
            }
        
            if(!refresh)
                smokePS.Play();

            initialized = true;
        }

        public void Grow()
        {
            travelInProgress = true;
        }
    
        // grow speed = (total len) / (total secs)
        // segment speed = (seg len) / (seg secs)
        // segment length = [ (total len) / # segs ] / [ (total secs) / # segs ]
        // segment secs = (total secs) / # segs

        bool NaturalGrowthUpdate()
        {
            int len = waypoints.Count - 1;

            bool completed = false;

            if (interval < 1f) 
            {
                float speed = growSpeed;
                if (index == 1)
                    speed = saplingSpeed;
            
                interval += (Time.deltaTime * speed);
                interval = Mathf.Clamp01(interval); // Clamp 0-1

                LerpVertex(index, interval);
                ParseAllLeaves(index, interval, false);
            }
            else 
            {
                interval = 0f;
            
                if (index == len) 
                {
                    completed = true;
                }
                else {
                    ++index;

                    AddVertexAtPoint(waypoints[index-1]);
                }
            }

            return completed;
        }

        bool GatedGrowthUpdate()
        {
            bool completed = true;

            for (int i = 0; i < waypoints.Count; i++) 
            {
                LerpVertex(i, Time.deltaTime * gateSpeed);
            }
        
            for(int j = 0; j < waypoints.Count; j++) {
                int ind = gateIndices[j];
                int tInd = gateTransition.Count - (waypoints.Count - 1 - j);
            
                if (ind < tInd) {
                    completed = false;
                    break;
                }   
            }

            return completed;
        }

        #endregion
    
        #region Vertex operations
    
        void AddVertexAtPoint(Vector3 point)
        {
            vertices.Add(point);
        }

        void LerpVertex(int index, float interval)
        {
            var resultant = vertices[index];

            if (state == Status.Natural) 
            {
                var current = vertices[index - 1];
                var target = waypoints[index];

                resultant = Vector3.Lerp(current, target, interval);
            }
            else {
                int tIndex = gateIndices[index];
                if (tIndex < gateTransition.Count) 
                {
                    Vector3 target = gateTransition[tIndex];

                    resultant = Vector3.Lerp(vertices[index], target, interval);
                    if (Vector3.Distance(resultant, target) <= snapDistance) 
                        gateIndices[index] = ++tIndex;
                }
            }

            vertices[index] = resultant;
        }

        Vector3 GetGatedVertexFromGateState(int index, int state)
        {
            Vector3 root = waypoints[index];
            Vector3 target = gate[index];

            Vector3 top = vertex.top;

            if (state == 0) 
            {
                return new Vector3(root.x, top.y, root.z);
            }   
            if (state == 1) {
                return top;
            }
            if (state == 2) {
                return target;
            }

            return root;
        }
    
        int GetVertexFromInterval(float i, IEnumerable<Vector3> line, ref float progress)
        {
            if (i <= 0f) return 0;
            if (i >= 1f) return line.Count()-1;
        
            i = Mathf.Clamp01(i);
        
            float length = line.DistanceBetweenVectors(); // Get length of line
            float lengthInterval = i * length;

            float distance = 0f;
            for (int index = 0; index < line.Count(); index++) {
                if (index > 0) 
                {
                    float amt = Vector3.Distance(line.ElementAt(index - 1), line.ElementAt(index));
                    float cacheDistance = distance;
                
                    distance += amt;
                    if (distance >= lengthInterval) {
                        progress = (lengthInterval - cacheDistance) / amt;
                        return index;
                    }
                }
            }
        
            return -1;
        }

        #endregion

        #region Construction

        Vector3[] ConstructNaturalLine()
        {
            int numberOfWaypoints = Random.Range(minSegments, maxSegments);

            List<Vector3> waypoints = new List<Vector3>();

            Vector3 origin = transform.position;
            Vector3 lastWaypoint = origin;
            Vector3 nextWaypoint = origin;

            float minHeight = (growHeight / maxSegments);
            float maxHeight = (growHeight / minSegments);

            float growOffset = (growHeight / numberOfWaypoints);
            float gh = 0f;

            int i = 0;
            while(gh <= growHeight) 
            {
                float h = Random.Range(minHeight, maxHeight);
            
                if (i > 0) // Past position
                { 
                    lastWaypoint = waypoints[waypoints.Count - 1];
                    nextWaypoint = NextWaypoint(origin, lastWaypoint, gh);
                }
                gh += h;

                waypoints.Add(nextWaypoint);
            
                ++i;
            
                if (gh >= growHeight)
                    break;
            }

            return waypoints.ToArray();
        }

        Vector3 NextWaypoint(Vector3 origin, Vector3 previous, float height)
        {
            //Vector2 pointRadius = Random.insideUnitCircle * growRadius;
            Vector2 pointRadius = Vector2.one * Mathf.PerlinNoise(previous.x, previous.y).RemapNRB(0f, 1f, -1f, 1f) * growRadius;
            Vector3 waypoint = transform.position + new Vector3(pointRadius.x, height, pointRadius.y);

            return waypoint;
        }


        Vector3[] ConstructGatedLine(Cage.Vertex vert)
        {
            var len = waypoints.Count;

            List<Vector3> vertices = new List<Vector3>();
            gateIndices = new List<int>();
            gateTransition = new List<Vector3>(waypoints);
        
            Vector3 a = waypoints.Last();
            Vector3 b = vertex.top;

            int transitioncount = Mathf.FloorToInt( Vector3.Distance(a, b) / growHeight * minSegments );
            for(int j = 1; j < transitioncount; j++)
                gateTransition.Add(Vector3.Lerp(a, b, j * 1f / transitioncount));

            for (int i = 0; i < len; i++) 
            {
                var interval = (1f * i / ( len-1 ) );
                var vertex = Vector3.Lerp(vert.top, vert.bottom, interval);
            
                vertices.Add(vertex);
            
                gateTransition.Add(vertex);
                gateIndices.Add(i + 1); // Assign transition position
            }

            return vertices.ToArray();
        }

        #endregion

        #region Walls

        GameObject CreateGate(Vector3 center, Vector2 bounds)
        {
            var instance = Instantiate(wallPrefab);
            var instance_t = instance.transform;

            var ptA = gate[1];
            var ptB = gate[2];

            instance_t.position = center;
            instance_t.rotation = Quaternion.LookRotation((ptB - ptA).normalized, transform.up);
            instance_t.localScale = new Vector3(instance_t.localScale.x, bounds.y, bounds.x);
            instance_t.parent = transform;

            return null;
        }

        public void TransitionToGate() 
        {
            state = Status.Gate;
            travelInProgress = true;
            interval = 0f;

            //CreateGate(gate_midpoint, new Vector2(gate_width, gate_height));
        }

        #endregion

        #region Collisions

        void UpdateColliderBounds()
        {
            if (line.positionCount > 0) {
                var height = Vector3.Distance(transform.position, end);
                var radius = growRadius;

                collider.height = height;
                collider.radius = radius;

                collider.center = new Vector3(0f, height/2f, 0f);

                collider.enabled = true;
            }
            else {
                collider.enabled = false;
            }
        }

        #endregion
    
        #region Flowers and leaves

        public void GrowFlower()
        {
            Vector3 pos = (state == Status.Natural) ? end : vertex.top;
        
            var flowerInstance = Instantiate(flowerPrefab, pos, flowerPrefab.transform.rotation);
            flowerInstance.transform.parent = transform;

            var flower = flowerInstance.GetComponentInChildren<Flower>();
            flower.Grow(Flower.Origin.Vine); 
        }

        Leaf[] ConstructAllLeaves()
        {
            if (waypoints == null || waypoints.Count == 0) return null;
        
            List<Leaf> l = new List<Leaf>();

            float density = preset.leafDensityPerSegment;
        
            int min = (int)(preset.minimumLeavesPerSegment*density);
            int max = (int)(preset.maximumLeavesPerSegment*density);

            for(int i = 0; i < waypoints.Count-1; i++) 
            {
                int count = Random.Range(min, max + 1);
                for(int j = 0; j < count; j++) 
                {
                    float pos = Random.Range(0f, 1f);

                    var leafInstance = ConstructLeaf(i, pos);
                    if (leafInstance != null) 
                        l.Add(leafInstance);
                } 
            }

            return l.ToArray();
        }

        Leaf ConstructLeaf(int index, float progress, float scale = -1f, float offset = -1f)
        {
            var leafInstance = GrowLeaf(index, progress,  ref offset);
            if (leafInstance != null) 
            {
                var leaf = leafInstance.GetComponent<Leaf>();
                leaf.index = index;
                leaf.progress = progress;
                leaf.offset = offset;
                leaf.Scale = (scale == -1f)? Random.Range(preset.minimumLeafScale, preset.maximumLeafScale):scale;

                return leaf;
            }

            return null;
        }

        void ParseAllLeaves(int index, float progress, bool immediate)
        {
            foreach (Leaf leaf in leaves) 
            {
                bool flag = (leaf.index < index);

                if (!flag && leaf.index == index) 
                    flag = leaf.progress < progress;

                if(flag && !leaf.Visible)
                    leaf.Show(immediate);
                else if(!flag && leaf.Visible)
                    leaf.Hide(immediate);
            }
        }
    
        public GameObject GrowLeaf(int indexA, float segment, ref float offset)
        {
            if (indexA < waypoints.Count - 1) 
            {
                int indexB = indexA + 1;

                Vector3 a = waypoints[indexA];
                Vector3 b = waypoints[indexB];
            
                Vector3 position = Vector3.Lerp(a, b, segment);
                Vector3 up = (b - a).normalized;

                offset = (offset == -1f) ? Random.Range(0f, 360f) : offset;
                Quaternion rot = Quaternion.AngleAxis(offset, up); // Rotate around vector

                GameObject instance = Instantiate(leafPrefab, position, rot);
                instance.transform.parent = transform;

                return instance;
            }

            offset = 0f;
            return null;
        }

        public void Flutter()
        {
            foreach(Leaf l in leaves)
                l.Flutter();
        }

        public void Unflutter()
        {
            foreach(Leaf l in leaves)
                l.Unflutter();
        }
    
        #endregion

        #region Info
    
        public string GetInfo()
        {
            return File;
        }
    
        #endregion
    }
}
