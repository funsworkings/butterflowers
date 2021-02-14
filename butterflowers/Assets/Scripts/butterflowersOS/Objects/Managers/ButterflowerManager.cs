using System.Collections;
using System.Collections.Generic;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Objects.Entities;
using butterflowersOS.Objects.Entities.Interactables;
using butterflowersOS.Presets;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using uwu.Extensions;
using uwu.Gameplay;
using uwu.Snippets.Load;
using Extensions = uwu.Extensions.Extensions;
using Random = UnityEngine.Random;

namespace butterflowersOS.Objects.Managers
{
    public class ButterflowerManager : Spawner, IReactToSunCycle, ILoadDependent
    {
        public static ButterflowerManager Instance = null;

        public UnityEvent onAllDead;
        public UnityEvent onSpawn, onDeath;

        public UnityEvent onSpawnBatch, onKillBatch;

        #region Internal

        public enum Op
        {
            Nothing = -1,

            Release = 1,
            Kill = 3
        }

        #endregion

        // External

        [SerializeField] new Camera camera;

        // Properties

        [SerializeField] WorldPreset preset = null;
        [SerializeField] ButterflyPreset butterflyPreset = null;
        [SerializeField] Wand wand = null;
        [SerializeField] Nest nest = null;
        [SerializeField] Quilt quilt = null;
        [SerializeField] Canvas canvas;
        [SerializeField] Transform variableSpawnRoot = null;
        [SerializeField] Transform currentRoot;

        [SerializeField] Op op = Op.Nothing;
        [SerializeField] int _op = -1;

        [SerializeField] int maxBatchSize = 10;

        int totalCount = 0;

        // Attributes

        bool respawn = true;

        [SerializeField] int alive = 0, dead = 0;
        [SerializeField] float health = 1f;

        [SerializeField] float healthChangeThreshold = .1f;

        [Header("Butterfly attributes")] [SerializeField]
        Mesh butterflyMesh;

        [SerializeField] Material butterflyMaterial;

        // Collections

        List<Butterfly> butterflies = new List<Butterfly>();

        NativeArray<int> states;
        NativeArray<float3> origins;
        NativeArray<float3> positions;
        NativeArray<float3> relPositions;
        NativeArray<float3> velocities;
        NativeArray<float> tsi;
        NativeArray<float> _wand;
        NativeArray<Unity.Mathematics.Random> _randoms;
        TransformAccessArray transforms;

        NativeArray<float> distanceCurve;
        NativeArray<float> speedCurve;
        NativeArray<float> deathCurve;

        // Jobs

        PreBuildButterflyJob m_PreBuildJob;
        JobHandle m_PreBuildJobHandle;

        VelocityButterflyJob m_VelocityJob;
        JobHandle m_VelocityJobHandle;

        ScaleButterflyJob m_ScaleJob;
        JobHandle m_ScaleJobHandle;


        public float Progress
        {
            get => (totalCount * 1f / amount);
        }

        public bool Completed
        {
            get => totalCount == amount;
        }

        public Transform VariableRoot => variableSpawnRoot;
        

        protected override void Awake()
        {
            Instance = this;

            base.Awake();
        }

        // Start is called before the first frame updaste
        protected override void Start()
        {
            Butterfly.OnRegister += AddButterfly;
            Butterfly.OnUnregister += RemoveButterfly;

            ResetToDefaultRoot(); // Set to default root
            CalculateBounds();
        }

        public void Load()
        {
            if (Completed) return;

            amount = (preset != null) ? preset.amountOfButterflies : 100;
            StartCoroutine("Spawning");
        }

        bool load = false;

        IEnumerator Spawning()
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject[] batch;
            int batchSize = 0;

            while (totalCount < amount) {
                batchSize = Random.Range(1, Mathf.Min(maxBatchSize, amount - totalCount));
                
                batch = Spawn(batchSize);
                objects.AddRange(batch);

                totalCount += batchSize;
                yield return new WaitForEndOfFrame();
            }

            var _transforms = objects.Select(e => e.transform).ToArray();
            var _butterflies = objects.Select(e => e.GetComponent<Butterfly>()).ToArray();

            states = new NativeArray<int>(amount, Allocator.Persistent);
            origins = new NativeArray<float3>(amount, Allocator.Persistent);
            positions = new NativeArray<float3>(amount, Allocator.Persistent);
            relPositions = new NativeArray<float3>(amount, Allocator.Persistent);
            velocities = new NativeArray<float3>(amount, Allocator.Persistent);
            tsi = new NativeArray<float>(amount, Allocator.Persistent);
            _wand = new NativeArray<float>(amount, Allocator.Persistent);
            transforms = new TransformAccessArray(_transforms);

            distanceCurve = new NativeArray<float>(butterflyPreset.distanceAttractionCurve.GenerateCurveArray(),
                Allocator.Persistent);
            speedCurve = new NativeArray<float>(butterflyPreset.speedAttractionCurve.GenerateCurveArray(),
                Allocator.Persistent);
            deathCurve = new NativeArray<float>(butterflyPreset.deathProbabilityCurve.GenerateCurveArray(),
                Allocator.Persistent);

            for (int i = 0; i < _butterflies.Length; i++) {
                origins[i] = _butterflies[i].origin;
                positions[i] = _butterflies[i].transform.position;
            }

            load = true;
        }

        protected override void Update()
        {
            base.Update();

            if (!Completed || !load) return; // Ignore update until spawned

            float dt = Time.deltaTime;

            // Bind new randoms
            _randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
            var r = (uint) Random.Range(int.MinValue, int.MaxValue);
            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
                _randoms[i] = new Unity.Mathematics.Random(r == 0 ? r + 1 : r);

            Camera camera = wand.Camera;
            Transform camera_t = camera.transform;

            m_PreBuildJob = new PreBuildButterflyJob() {
                op = (int) op,

                relPosition = relPositions,
                state = states,
                wand = _wand,
                tsi = tsi,

                origin = origins,
                position = positions,

                deltaTime = dt,

                cameraPosition = camera_t.position,
                cameraMatrix = camera.projectionMatrix,
                cameraUp = camera_t.up,
                cameraRight = camera_t.right,
                cameraForward = camera_t.forward,
                pixelWidth = camera.scaledPixelWidth,
                pixelHeight = camera.scaledPixelHeight,
                scaleFactor = 1f,

                wandTrajectory2d = wand.trajectory2d,
                wandRadius = wand.radius,
                wandSpeed = wand.speed,
                minWandSpeed = butterflyPreset.minimumWandSpeed,
                maxWandSpeed = butterflyPreset.maximumWandSpeed,

                quiltSpeed = quilt.speedInterval,
                deathCurve = deathCurve,
                timeToDie = butterflyPreset.timeDead,

                nestOpen = (nest.open) ? 1 : 0,
                nestPosition = nest.transform.position
            };

            m_VelocityJob = new VelocityButterflyJob() {
                state = states,
                origin = origins,
                position = positions,
                positionRelCamera = relPositions,
                timeInState = tsi,
                wand = _wand,

                velocity = velocities,

                deltaTime = dt,
                distanceCurve = distanceCurve,
                speedCurve = speedCurve,
                gravity = butterflyPreset.gravity,
                noiseSize = butterflyPreset.noiseSize,
                noiseAmount = butterflyPreset.noiseAmount,
                dampening = butterflyPreset.velocityDampening,
                attraction = butterflyPreset.attraction,
                maxSpeed = butterflyPreset.maxSpeed,
                minWandSpeed = butterflyPreset.minimumWandSpeed,
                maxWandSpeed = butterflyPreset.maximumWandSpeed,

                wandVelocity3d = wand.velocity3d,
                wandSpeed = wand.speed,

                nestPosition = nest.transform.position,
                cameraPosition = camera_t.position
            };

            m_ScaleJob = new ScaleButterflyJob() {
                state = states,
                timeInState = tsi,
                relPosition = relPositions,

                scale = butterflyPreset.scale,
                growTime = butterflyPreset.timeToGrow,

                wandPosition3d = wand.position3d,
                wandRadius = wand.radius,

                nestEnergy = nest.energy,
                energyGrowth = butterflyPreset.energyGrowth
            };


            m_PreBuildJobHandle = m_PreBuildJob.Schedule(amount, 2);
            m_VelocityJobHandle = m_VelocityJob.Schedule(transforms, m_PreBuildJobHandle);
            m_ScaleJobHandle = m_ScaleJob.Schedule(transforms, m_VelocityJobHandle);
            
            m_ScaleJobHandle.Complete();


            float previousHealth = health;
            health = GetHealth();

            float diff = (health - previousHealth);
            if (Mathf.Abs(diff) > healthChangeThreshold) {
                if(diff < 0f) onKillBatch.Invoke();
                else onSpawnBatch.Invoke();
            }

            _randoms.Dispose(); // Wipe random values

            // Clean up frame ops
            if (_op > 0) 
            {
                op = (Op) _op;
                _op = -1;
            }
            else {
                op = Op.Nothing;
            }

        }

        protected override void OnDestroy()
        {
            Butterfly.OnRegister -= AddButterfly;
            Butterfly.OnUnregister -= RemoveButterfly;


            // Dispose all native arrays (JOBS)

            states.Dispose();
            origins.Dispose();
            positions.Dispose();
            relPositions.Dispose();
            velocities.Dispose();
            _wand.Dispose();
            tsi.Dispose();
            transforms.Dispose();

            distanceCurve.Dispose();
            speedCurve.Dispose();
            deathCurve.Dispose();

            if(_randoms.IsCreated) _randoms.Dispose();
            
            
            base.OnDestroy();
        }

        #region Cycle

        public void Cycle(bool refresh)
        {
            KillButterflies();
        }

        #endregion

        #region Root

        public void UseVariableRoot()
        {
            currentRoot = variableSpawnRoot;
        }

        public void ResetToDefaultRoot()
        {
            currentRoot = root;
        }

        #endregion

        #region Spawner overrides

        protected override Transform spawnRoot => currentRoot;

        void ResetButterfly(Butterfly butterfly)
        {
            if (alive == 0) onAllDead.Invoke();

            if (!respawn) return;
            butterfly.Reset();
        }
        
        protected override void CalculateBounds()
        {
            var col = spawnRoot.GetComponent<Collider>();

            m_center = spawnRoot.InverseTransformPoint(col.bounds.center);
            m_extents = spawnRoot.InverseTransformVector(col.bounds.extents);

            col.enabled = false; // Disable collider after fetching center+bounds
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
            if (refresh) 
                dead--;
        }
        
        public void ResetAllOrigins()
        {
            CalculateBounds(); // Re-caclulate bounds!
            
            for (int i = 0; i < butterflies.Count; i++) 
            {
                var _butterfly = butterflies[i];
                
                var _origin = _butterfly.origin;
                DecidePosition(ref _origin);

                origins[i] = _butterfly.origin = _origin;
            }
        }


        #endregion

        #region Butterfly operations

        bool open = false;

        public void KillButterflies(bool infinite = true)
        {
            respawn = infinite;
            _op = (int)Op.Kill;

            if (open) {
                onDeath.Invoke();
                open = false;
            }
        }

        public void ReleaseButterflies()
        {
            _op = (int)Op.Release;

            if (!open) 
            {
                onSpawn.Invoke();
                open = true;
            }
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

            int index = butterfly.transform.GetSiblingIndex();
            Butterfly.State state = (Butterfly.State)states[index];
        
            if (state == Butterfly.State.Dying) 
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

        #region Health

        public float GetHealth()
        {
            int alive = 0, dead = 0;
            foreach (int state in states) 
            {
                if (state == 3) // Dying 
                    dead++;
                else
                    alive++;
            }
        
            int total = (alive + dead);
            if (total == 0) return 1f;

            return ((float)alive / total);
        }

        #endregion
    
        #region Jobs

        [BurstCompile]
        struct PreBuildButterflyJob : IJobParallelFor
        {
            [ReadOnly] public int op;

            public NativeArray<float3> relPosition;
            public NativeArray<int> state;
            public NativeArray<float> wand;
            public NativeArray<float> tsi;

            [ReadOnly] public NativeArray<float3> origin;
        
            public NativeArray<float3> position;

            //[NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> randoms;
            //[NativeSetThreadIndex] int _threadID;

            [ReadOnly] public float deltaTime;
        
            [ReadOnly] public float3 cameraPosition;
            [ReadOnly] public float4x4 cameraMatrix;
            [ReadOnly] public float3 cameraUp, cameraForward, cameraRight;
            [ReadOnly] public float pixelWidth, pixelHeight, scaleFactor;
            
            [ReadOnly] public float3 wandTrajectory2d;
            [ReadOnly] public float wandRadius;
            [ReadOnly] public float wandSpeed;
            [ReadOnly] public float minWandSpeed, maxWandSpeed;

            [ReadOnly] public float quiltSpeed;

            [ReadOnly] public Vector3 nestPosition;
            [ReadOnly] public int nestOpen;
        
            [ReadOnly] public NativeArray<float> deathCurve;
            [ReadOnly] public float timeToDie;

            public void Execute(int index)
            {
                ExecuteRelativePosition(index);
                ExecuteWandStrength(index);
                ExecuteStateChanges(index);
            }
        
            void ExecuteRelativePosition(int index)
            {
                float2 scpt = Extensions.ConvertWorldToScreenCoordinates(position[index],
                    cameraPosition,
                    cameraMatrix,
                    cameraUp,
                    cameraRight,
                    cameraForward,
                    pixelWidth, pixelHeight, scaleFactor);
            
                relPosition[index] = new float3(scpt.x, scpt.y, 0);
            }

            void ExecuteWandStrength(int index)
            {
                float3 target = wandTrajectory2d;
                float3 current = relPosition[index];

                float3 direction = (target - current);
        
                float radius = wandRadius;
                float dist = math.distance(float3.zero, direction);
            
                wand[index] = dist / radius;
            }

            void IncrementTSI(int index)
            {
                tsi[index] += deltaTime;
            }
        
            void ExecuteStateChanges(int index)
            {
                int previousState = state[index];
            
                if (op == -1) // Null op
                {
                    if (state[index] == 1) // Easing
                    {
                        if (CheckForArrival(index)) 
                            state[index] = 2; // Has arrived!
                    }
                    else if (state[index] == 2) // Alive
                    {
                        if (CheckForWandDeath(index))
                            state[index] = 3;

                        //if (CheckForQuiltDeath(index) || CheckForWandDeath(index)) state[index] = 2;
                    }
                    else if (state[index] == 3) // Dying
                    {
                        if (CheckForCompleteDeath(index)) 
                        {
                            if (nestOpen == 1) 
                            {
                                state[index] = 2;
                                position[index] = origin[index];
                            }
                            else 
                            {
                                state[index] = 0;
                                position[index] = nestPosition;
                            }
                        
                        }
                    }
                }
                else 
                {
                    state[index] = op;
                }

                IncrementTSI(index); // Increment time within state
                if (previousState != state[index]) tsi[index] = 0f; // Reset TSI
            }

            bool CheckForArrival(int index)
            {
                float distance = math.distance(position[index], origin[index]);
                return distance < 1f;
            }

            bool CheckForWandDeath(int index)
            {
                if (wand[index] <= 1f) 
                {
                    float minSpeed = minWandSpeed;
                    float maxSpeed = maxWandSpeed;
            
                    var wandSpeedInterval = (wandSpeed - minSpeed) / (maxSpeed - minSpeed);
                    return (wandSpeedInterval > 1f);
                }
            
                return false;
            }

            bool CheckForCompleteDeath(int index)
            {
                return tsi[index] > timeToDie;
            }

            /*
        bool CheckForQuiltDeath(int index)
        {
            int deathIndex = (int) (math.clamp(quiltSpeed, 0f, 1f) * 256);
            float probability = randoms[_threadID].NextFloat(0f, 1f);

            return probability < deathCurve[deathIndex];
        }
        */
        }

        [BurstCompile]
        struct VelocityButterflyJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<int> state;
            [ReadOnly] public NativeArray<float3> origin;
            [ReadOnly] public NativeArray<float3> positionRelCamera;
            [ReadOnly] public NativeArray<float> wand;
            [ReadOnly] public NativeArray<float> timeInState;
        
            public NativeArray<float3> velocity;
            public NativeArray<float3> position;

            //[NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> randoms;
            //[NativeSetThreadIndex] int _threadID;
        
            [ReadOnly] public NativeArray<float> distanceCurve, speedCurve;
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float gravity;
            [ReadOnly] public float noiseSize;
            [ReadOnly] public float noiseAmount;
            [ReadOnly] public float dampening;
            [ReadOnly] public float attraction;
            [ReadOnly] public float maxSpeed;
            [ReadOnly] public float minWandSpeed, maxWandSpeed;

            [ReadOnly] public float3 wandVelocity3d;
            [ReadOnly] public float wandSpeed;

            [ReadOnly] public float3 nestPosition;
            [ReadOnly] public float3 cameraPosition;

            public void Execute(int index, TransformAccess transform)
            {
                bool dampen = false;
            
                if (state[index] == 0) // Hidden
                { 
                    velocity[index] = float3.zero;
                }
                else if (state[index] == 1) // Easing
                {
                    float3 dir = origin[index] - position[index];
                    velocity[index] = dir;
                }
                else if (state[index] == 2) // Alive
                {
                    float strength = MoveWithWand(index);

                    // Move with noise
                    //MoveWithNoise(index);
                    if (strength <= 0f) dampen = true;
                    ClampVelocity(index);
                }
                else  // Dying
                {
                    velocity[index] += new float3(0, -1, 0) * (gravity + math.pow(timeInState[index], 4f));
                }
            
                if(dampen) velocity[index] *= (1f - dampening * deltaTime); // Dampen velocity\

                if (state[index] != 0) 
                {
                    float3 vel = velocity[index] * deltaTime;
                    transform.position = position[index] + vel;
                }
                else
                    transform.position = nestPosition;

                position[index] = transform.position; // Write to position array
            
                if(state[index] == 1 || state[index] == 2) FaceCamera(index, transform);
            }

            float MoveWithWand(int index)
            {
                float wand = this.wand[index];
            
                var attract = 0f;
                if (wand <= 1f) {
                    var distanceIndex = (int) (math.clamp(wand, 0f, 1f) * 256);
                    var distanceMagnitude = distanceCurve[distanceIndex];

                    float minSpeed = minWandSpeed;
                    float maxSpeed = maxWandSpeed;
            
                    var wandSpeedInterval = (wandSpeed - minSpeed) / (maxSpeed - minSpeed);
                    if (wandSpeedInterval > 1f)  //Check if killing butterfly, SPEED TOO GREAT
                    {
                        attract = distanceMagnitude;
                    }
                    else {
                        var speedIndex = (int)(math.clamp(wandSpeedInterval, 0f, 1f) * 256);
                        var speedMagnitude = speedCurve[speedIndex];
                    
                        attract = speedMagnitude * distanceMagnitude;
                    }

                    velocity[index] += wandVelocity3d * attract * attraction;
                }
            
                return attract;
            }

            /*
        void MoveWithNoise(int index)
        {
            float3 screen = positionRelCamera[index] * noiseSize;
            float _noise = Mathf.PerlinNoise(screen.x, screen.y);

            var r = randoms[_threadID];
            float3 noiseDir = r.NextFloat3Direction();
            
            float speed = noiseAmount * _noise;
            velocity[index] += (noiseDir * speed * deltaTime);
        }
        */

            void ClampVelocity(int index)
            {
                float3 vel = velocity[index];
                float magnitude = vel.x * vel.x + vel.y * vel.y + vel.z * vel.z;
            
                if (magnitude > 0f) 
                {
                    float speed = math.sqrt(magnitude);
                    speed = math.min(speed, maxSpeed);
                
                    velocity[index] = (speed *  math.normalize(velocity[index]));
                }
            }

            void FaceCamera(int index, TransformAccess transform)
            {
                float3 dir = math.normalize(cameraPosition - position[index]);
                quaternion rot = quaternion.LookRotationSafe(dir, new float3(0, 1, 0));

                transform.rotation = rot;
            }
        }

        [BurstCompile]
        struct ScaleButterflyJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<int> state;
            [ReadOnly] public NativeArray<float3> relPosition;
            [ReadOnly] public NativeArray<float> timeInState;
        
            [ReadOnly] public float scale;
            [ReadOnly] public float growTime;

            [ReadOnly] public float3 wandPosition3d;
            [ReadOnly] public float wandRadius;

            [ReadOnly] public float nestEnergy;
            [ReadOnly] public float energyGrowth;

            public void Execute(int index, TransformAccess transform)
            {
                float _scale = scale;
            
                if (state[index] == 0) 
                {
                    _scale = 0f;
                }
                else 
                {
                    if (state[index] == 1 && timeInState[index] < growTime) 
                    {
                        float i = timeInState[index] / growTime;
                        _scale = math.lerp(0f, scale, math.pow(i, 2f));
                    }
                    else 
                    {
                        float3 dir = wandPosition3d - relPosition[index];
                        float magnitude = math.clamp(1f - math.length(dir) / (wandRadius * 6f), 0f, 1f) +
                                          (nestEnergy * energyGrowth);

                        _scale *= (1f + math.pow(magnitude, 2f));
                    }
                }
            
                transform.localScale = Vector3.one * _scale;
            }
        }
    
        #endregion
    }
}
