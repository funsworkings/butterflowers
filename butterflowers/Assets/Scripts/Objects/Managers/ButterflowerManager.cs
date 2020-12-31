using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

using Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Events;
using uwu.Gameplay;
using UnityEngine.Jobs;
using uwu.Extensions;
using Extensions = uwu.Extensions.Extensions;
using Random = UnityEngine.Random;
using World = Unity.Entities.World;

public class ButterflowerManager : Spawner, IReactToSunCycle
{
    public static ButterflowerManager Instance = null;

    public UnityEvent onAllDead;
    
    #region Internal

    public enum Op
    {
        Nothing = -1,
        
        Kill = 0,
        Release = 1
    }
    
    #endregion

    // External

    [SerializeField] new Camera camera;

    // Properties

    [SerializeField] WorldPreset preset = null;
    [SerializeField] ButterflyPreset butterflyPreset;
    [SerializeField] Wand wand;
    [SerializeField] Nest nest;
    [SerializeField] Quilt quilt;
    [SerializeField] Canvas canvas;

    [SerializeField] Op op = Op.Nothing;

    // Attributes

    bool respawn = true;
    
    [SerializeField] int alive = 0, dead = 0;

    [Header("Butterfly attributes")] 
        [SerializeField] Mesh butterflyMesh;
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

    protected override void Awake()
    {
        Instance = this;

        amount = (preset != null)? preset.amountOfButterflies:100;

        base.Awake();
    }

    // Start is called before the first frame updaste
    protected override void Start()
    {
        Butterfly.OnRegister += AddButterfly;
        Butterfly.OnUnregister += RemoveButterfly;

        Butterfly.Dying += ButterflyDying;
        Butterfly.Died += ResetButterfly;

        CalculateBounds();

        var entities = Spawn(amount);
        var _transforms = entities.Select(e => e.transform).ToArray();

        states = new NativeArray<int>(amount, Allocator.Persistent);
        origins = new NativeArray<float3>(amount, Allocator.Persistent);
        positions = new NativeArray<float3>(amount, Allocator.Persistent);
        relPositions = new NativeArray<float3>(amount, Allocator.Persistent);
        velocities = new NativeArray<float3>(amount, Allocator.Persistent);
        tsi = new NativeArray<float>(amount, Allocator.Persistent);
        _wand = new NativeArray<float>(amount, Allocator.Persistent);
        transforms = new TransformAccessArray(_transforms);
        
        distanceCurve = new NativeArray<float>(butterflyPreset.distanceAttractionCurve.GenerateCurveArray(), Allocator.Persistent);
        speedCurve = new NativeArray<float>(butterflyPreset.speedAttractionCurve.GenerateCurveArray(), Allocator.Persistent);
        deathCurve = new NativeArray<float>(butterflyPreset.deathProbabilityCurve.GenerateCurveArray(), Allocator.Persistent);;
    }

    protected override void Update()
    {
        base.Update();

        for (int i = 0; i < butterflies.Count; i++) 
        {
            states[i] = (int)butterflies[i]._State;
            origins[i] = butterflies[i].origin;
            positions[i] = butterflies[i].transform.position;
            velocities[i] = butterflies[i].Velocity;
            tsi[i] = butterflies[i].TimeInState;
        }

        float dt = Time.deltaTime;
        
        // Bind new randoms
        _randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        var r = (uint) Random.Range(int.MinValue, int.MaxValue);
        for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            _randoms[i] = new Unity.Mathematics.Random(r == 0 ? r + 1 : r);
        

        Camera camera = wand.Camera;
        Transform camera_t = camera.transform;

        m_PreBuildJob = new PreBuildButterflyJob() 
        {
            op = (int)op,
            
            relPosition = relPositions,
            state = states,
            wand = _wand,
            
            origin = origins,
            position = positions,

            cameraPosition = camera_t.position,
            cameraMatrix = camera.projectionMatrix,
            cameraUp = camera_t.up,
            cameraRight = camera_t.right,
            cameraForward = camera_t.forward,
            pixelWidth = camera.pixelWidth,
            pixelHeight = camera.pixelHeight,
            scaleFactor = canvas.scaleFactor,
            
            wandTrajectory2d = wand.trajectory2d,
            wandRadius = wand.radius,
            wandSpeed = wand.speed,
            minWandSpeed = butterflyPreset.minimumWandSpeed,
            maxWandSpeed = butterflyPreset.maximumWandSpeed,
            
            quiltSpeed = quilt.speedInterval,
            deathCurve = deathCurve
        };

        m_PreBuildJobHandle = m_PreBuildJob.Schedule(amount, 1);
        
        if (op == Op.Nothing)  // Do not apply velocity OR scale updates in op during frame
        {
            m_VelocityJob = new VelocityButterflyJob() 
            {
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
            
                nestPosition = nest.transform.position
            };

            m_ScaleJob = new ScaleButterflyJob() 
            {
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
            
            
            m_VelocityJobHandle = m_VelocityJob.Schedule(transforms, m_PreBuildJobHandle);
            m_ScaleJobHandle = m_ScaleJob.Schedule(transforms, m_PreBuildJobHandle);
            
            m_VelocityJobHandle.Complete();
            m_ScaleJobHandle.Complete();
        }
        else 
        {
            m_PreBuildJobHandle.Complete();    
        }
        
        _randoms.Dispose(); // Wipe random values
        
        
        // Assignment!

        for (int i = 0; i < amount; i++) 
        {
            butterflies[i]._State = (Butterfly.State) states[i]; // Assign state
            
            butterflies[i].positionRelativeToCamera = relPositions[i]; // Assign position rel camera
            butterflies[i].Velocity = velocities[i]; // Assign velocity
        }

        op = Op.Nothing;
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        Butterfly.OnRegister -= AddButterfly;
        Butterfly.OnUnregister -= RemoveButterfly;

        Butterfly.Dying -= ButterflyDying;
        Butterfly.Died -= ResetButterfly;

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

        _randoms.Dispose();
    }

    #region Cycle

    public void Cycle(bool refresh)
    {
        KillButterflies();
    }
    
    #endregion

    #region Spawner overrides

    void ResetButterfly(Butterfly butterfly)
    {
        if (alive == 0) onAllDead.Invoke();

        if (!respawn) return;
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
        if (refresh) 
            dead--;
    }

    #endregion

    #region Butterfly operations

    public void KillButterflies(bool infinite = true)
    {
        respawn = infinite;
        op = Op.Kill;
    }

    public void ReleaseButterflies()
    {
        op = Op.Release;
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

    #region Health

    public float GetHealth()
    {
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

        [ReadOnly] public NativeArray<float3> origin;
        [ReadOnly] public NativeArray<float3> position;

        //[NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> randoms;
        //[NativeSetThreadIndex] int _threadID;
        
        [ReadOnly] public float3 cameraPosition;
        [ReadOnly] public float4x4 cameraMatrix;
        [ReadOnly] public float3 cameraUp, cameraForward, cameraRight;
        [ReadOnly] public float pixelWidth, pixelHeight, scaleFactor;
            
        [ReadOnly] public float3 wandTrajectory2d;
        [ReadOnly] public float wandRadius;
        [ReadOnly] public float wandSpeed;
        [ReadOnly] public float minWandSpeed, maxWandSpeed;

        [ReadOnly] public float quiltSpeed;
        [ReadOnly] public NativeArray<float> deathCurve;

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
        
        void ExecuteStateChanges(int index)
        {
            if (op == -1) // Null op
            {
                if (state[index] == 0) // Easing
                {
                    if (CheckForArrival(index)) state[index] = 1; // Has arrived!
                }
                else if (state[index] == 1) // Alive
                {
                    if( CheckForWandDeath(index)) state[index] = 2;
                    //if (CheckForQuiltDeath(index) || CheckForWandDeath(index)) state[index] = 2;
                }
            }
            else 
            {
                state[index] = op;
            }
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
        [ReadOnly] public NativeArray<float3> position;
        [ReadOnly] public NativeArray<float> wand;
        [ReadOnly] public NativeArray<float> timeInState;
        
        public NativeArray<float3> velocity;

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

        public void Execute(int index, TransformAccess transform)
        {
            bool dampen = false;
            
            if (state[index] == -1) // Hidden
            { 
                velocity[index] = float3.zero;
            }
            else if (state[index] == 0) // Easing
            {
                float3 dir = origin[index] - position[index];
                velocity[index] = dir;
            }
            else if (state[index] == 1) // Alive
            {
                float strength = MoveWithWand(index);

                // Move with noise
                //MoveWithNoise(index);
                if (strength <= 0f)
                    dampen = true;
            }
            else  // Dying
            {
                velocity[index] += (new float3(0, -1, 0) * gravity);
            }

            if(state[index] < 2) // Not dying
                ClampVelocity(index);

            if(dampen) velocity[index] *= (1f - dampening * deltaTime); // Dampen velocity\

            /*if (state[index] != -1) {
                float3 vel = velocity[index] * deltaTime;
                Vector3 offset = new Vector3(vel.x, vel.y, vel.z);

                transform.position += offset;
            }
            else*/
                transform.position = nestPosition;
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
            float speed = math.length(velocity[index]);
            speed = math.min(speed, maxSpeed);
            
            velocity[index] = (speed *  math.normalize(velocity[index]));
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
            
            if (state[index] == -1) 
            {
                _scale = 0f;
            }
            else 
            {
                if (state[index] == 0 && timeInState[index] < growTime) 
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
            
            transform.localScale = new float3(1f,1f,1f) * _scale;
        }
    }
    
    #endregion
}
