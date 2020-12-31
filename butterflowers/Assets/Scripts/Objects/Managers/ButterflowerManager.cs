using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

using UnityEngine.Networking;
using Settings;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Events;
using uwu.Gameplay;
using UnityEngine.Jobs;
using uwu.Extensions;
using Extensions = uwu.Extensions.Extensions;
using Random = UnityEngine.Random;

public class ButterflowerManager : Spawner, IReactToSunCycle
{
    public static ButterflowerManager Instance = null;

    public UnityEvent onAllDead;

    #region External

    [SerializeField] new Camera camera;

    #endregion
    
    // Properties

    [SerializeField] WorldPreset preset = null;
    [SerializeField] ButterflyPreset butterflyPreset;
    [SerializeField] Wand wand;
    [SerializeField] Canvas canvas;

    // Attributes

    bool respawn = true;

    [SerializeField] float offsetDistance = 0f;
    [SerializeField] int alive = 0, dead = 0;

    [SerializeField] int player_kills = 0, wizard_kills = 0, shared_kills = 0, misc_kills = 0;

    public float player_hatred => (dead > 0) ? (float)(player_kills + shared_kills/2F) / dead: 0F;
    public float wiz_hatred => (dead > 0)? (float)(wizard_kills + shared_kills/2F) / dead: 0F;
    public float shared_hatred => (dead > 0)? (float)shared_kills / dead: 0F;
    public float enviro_hated => (dead > 0) ? (float)misc_kills / dead : 0F;

    public float minKillDistance => offsetDistance/2f;

    // Collections

    List<Butterfly> butterflies = new List<Butterfly>();

    NativeArray<int> states;
    NativeArray<Vector3> origins;
    NativeArray<Vector3> positions;
    NativeArray<Vector3> relPositions;
    NativeArray<Vector3> velocities;
    NativeArray<Unity.Mathematics.Random> _randoms;
    TransformAccessArray transforms;

    NativeArray<float> distanceCurve;
    NativeArray<float> speedCurve;

    // Jobs

    RelPositionButterflyJob m_RelPosJob;
    JobHandle m_RelPosJobHandle;

    VelocityButterflyJob m_VelocityJob;
    JobHandle m_VelocityJobHandle;
    
    TranslateButterflyJob m_TranslateJob;
    JobHandle m_TranslateJobHandle;
    

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
        origins = new NativeArray<Vector3>(amount, Allocator.Persistent);
        positions = new NativeArray<Vector3>(amount, Allocator.Persistent);
        relPositions = new NativeArray<Vector3>(amount, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(amount, Allocator.Persistent);
        transforms = new TransformAccessArray(_transforms);
        
        distanceCurve = new NativeArray<float>(butterflyPreset.distanceAttractionCurve.GenerateCurveArray(), Allocator.Persistent);
        speedCurve = new NativeArray<float>(butterflyPreset.speedAttractionCurve.GenerateCurveArray(), Allocator.Persistent);
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
        }

        float dt = Time.deltaTime;

        Camera camera = wand.Camera;
        Transform camera_t = camera.transform;

        m_RelPosJob = new RelPositionButterflyJob() 
        {
            relPosition = relPositions,
            
            position = positions,
            cameraPosition = camera_t.position,
            cameraMatrix = camera.projectionMatrix,
            cameraUp = camera_t.up,
            cameraRight = camera_t.right,
            cameraForward = camera_t.forward,
            
            pixelWidth = camera.pixelWidth,
            pixelHeight = camera.pixelHeight,
            scaleFactor = canvas.scaleFactor
        };
        
        
        _randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        var r = (uint) Random.Range(int.MinValue, int.MaxValue);
        for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            _randoms[i] = new Unity.Mathematics.Random(r == 0 ? r + 1 : r);

        m_VelocityJob = new VelocityButterflyJob() 
        {
            state = states,
            origin = origins,
            position = positions,
            positionRelCamera = relPositions,
            velocity = velocities,
            
            randoms = _randoms,
            
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
            
            wandTrajectory = wand.trajectory2d,
            wandVelocity3d = wand.velocity3d,
            wandRadius = wand.radius,
            wandSpeed = wand.speed
        };

        m_TranslateJob = new TranslateButterflyJob() 
        {
            velocity = velocities,
            deltaTime = dt
        };

        m_RelPosJobHandle = m_RelPosJob.Schedule(amount, 1);
        m_VelocityJobHandle = m_VelocityJob.Schedule(amount, 64, m_RelPosJobHandle);
        m_TranslateJobHandle = m_TranslateJob.Schedule(transforms, m_VelocityJobHandle);
    }

    void LateUpdate()
    {
        m_TranslateJobHandle.Complete();
        
        for (int i = 0; i < amount; i++) 
        {
            butterflies[i].positionRelativeToCamera = relPositions[i];
            butterflies[i].Velocity = velocities[i];
        }

        _randoms.Dispose();
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
        transforms.Dispose();

        distanceCurve.Dispose();
        speedCurve.Dispose();

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
        if (alive == 0)
            onAllDead.Invoke();

        if (!respawn) return;

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
        if (refresh) {
            dead--;

            var b = obj.GetComponent<Butterfly>();
            SubDying(b);
        }
    }

    #endregion

    #region Butterfly operations

    public void KillButterflies(bool infinite = true)
    {
        respawn = infinite;
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

        if (butterfly.state == Butterfly.State.Dying) {
            dead--;
            SubDying(butterfly);
        }
        else
            alive--;
    }

    void ButterflyDying(Butterfly butterfly)
    {
        ++dead;
        AddDying(butterfly);

        --alive;
    }

    #endregion

    #region Health operations

    void AddDying(Butterfly butterfly)
    {
        if (butterfly.agent == AGENT.User)
            ++player_kills;
        else if (butterfly.agent == AGENT.Wizard)
            ++wizard_kills;
        else if (butterfly.agent == AGENT.Inhabitants)
            ++shared_kills;
        else
            ++misc_kills;
    }

    void SubDying(Butterfly butterfly)
    {
        if (butterfly.agent == AGENT.User)
            --player_kills;
        else if (butterfly.agent == AGENT.Wizard)
            --wizard_kills;
        else if (butterfly.agent == AGENT.Inhabitants)
            --shared_kills;
        else
            --misc_kills;
    }

    public float GetHealth()
    {
        int total = (alive + dead);
        if (total == 0) return 1f;

        return ((float)alive / total);
    }

	#endregion
    
    #region Jobs

    [BurstCompile]
    struct RelPositionButterflyJob : IJobParallelFor
    {
        public NativeArray<Vector3> relPosition;

        [ReadOnly] public NativeArray<Vector3> position;
        [ReadOnly] public Vector3 cameraPosition;
        [ReadOnly] public float4x4 cameraMatrix;
        [ReadOnly] public Vector3 cameraUp, cameraForward, cameraRight;
        [ReadOnly] public float pixelWidth, pixelHeight, scaleFactor;

        public void Execute(int index)
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
    }

    [BurstCompile]
    struct VelocityButterflyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> state;
        [ReadOnly] public NativeArray<Vector3> origin;
        [ReadOnly] public NativeArray<Vector3> positionRelCamera;
        [ReadOnly] public NativeArray<Vector3> position;
        
        public NativeArray<Vector3> velocity;

        [NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> randoms;
        [NativeSetThreadIndex] int _threadID;
        
        [ReadOnly] public NativeArray<float> distanceCurve, speedCurve;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float gravity;
        [ReadOnly] public float noiseSize;
        [ReadOnly] public float noiseAmount;
        [ReadOnly] public float dampening;
        [ReadOnly] public float attraction;
        [ReadOnly] public float maxSpeed;
        [ReadOnly] public float minWandSpeed, maxWandSpeed;

        [ReadOnly] public Vector3 wandTrajectory, wandVelocity3d;
        [ReadOnly] public float wandRadius;
        [ReadOnly] public float wandSpeed;

        public void Execute(int index)
        {
            bool dampen = false;
            
            if (state[index] == -1) // Hidden
            { 
                velocity[index] = Vector3.zero;
            }
            else if (state[index] == 0) // Easing
            {
                Vector3 dir = origin[index] - position[index];
                velocity[index] = dir;
            }
            else if (state[index] == 1) // Alive
            {
                float strength = MoveWithWand(index);

                // Move with noise
                MoveWithNoise(index);
                if (strength <= 0f)
                    dampen = true;
            }
            else  // Dying
            {
                velocity[index] += (Vector3.down * gravity);
            }

            if(state[index] < 2) // Not dying
                ClampVelocity(index);

            if(dampen) velocity[index] *= (1f - dampening * deltaTime); // Dampen velocity
        }

        float MoveWithWand(int index)
        {
            Vector3 direction = wandTrajectory - positionRelCamera[index];
            float offset = direction.magnitude / wandRadius;
            
            var attract = 0f;
            if (offset <= 1f) {
                var distanceIndex = (int) (Mathf.Clamp01(offset) * 256);
                var distanceMagnitude = distanceCurve[distanceIndex];

                float minSpeed = minWandSpeed;
                float maxSpeed = maxWandSpeed;
            
                var wandSpeedInterval = (wandSpeed - minSpeed) / (maxSpeed - minSpeed);
                if (wandSpeedInterval > 1f)  //Check if killing butterfly, SPEED TOO GREAT
                {
                    attract = distanceMagnitude;
                }
                else {
                    var speedIndex = (int)(Mathf.Clamp01(wandSpeedInterval) * 256);
                    var speedMagnitude = speedCurve[speedIndex];
                    
                    attract = speedMagnitude * distanceMagnitude;
                }

                velocity[index] += wandVelocity3d * attract * attraction;
            }
            
            return attract;
        }

        void MoveWithNoise(int index)
        {
            Vector2 screen = positionRelCamera[index] * noiseSize;
            float _noise = Mathf.PerlinNoise(screen.x, screen.y);

            var r = randoms[_threadID];
            Vector3 noiseDir = r.NextFloat3Direction();
            
            float speed = noiseAmount * _noise;
            velocity[index] += (noiseDir * speed * deltaTime);
        }

        void ClampVelocity(int index)
        {
            float speed = velocity[index].magnitude;
            speed = Mathf.Min(speed, maxSpeed);

            velocity[index] = (speed * velocity[index].normalized);
        }
    }

    [BurstCompile]
    struct TranslateButterflyJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<Vector3> velocity;
        [ReadOnly] public float deltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 vel = velocity[index] * deltaTime;
            transform.position += vel;
        }
    }

    [BurstCompile]
    struct ScaleButterflyJob : IJobParallelForTransform
    {
        public NativeArray<float3> scale;
        [ReadOnly] public float deltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            scale[index] += new float3(deltaTime, deltaTime, deltaTime);
            transform.localScale = scale[index];
        }
    }
    
    #endregion
}
