using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Rendering;

using Settings;
using uwu.Camera;
using Preset = Settings.ButterflyPreset;

public class Butterfly : MonoBehaviour
{
    #region Internal
    
    public enum State
    {
        Hidden = -1,
        Easing = 0,
        Alive = 1,
        Dying = 2
    }
    
    #endregion
    
    // Events
    
    public static System.Action<Butterfly> OnRegister, OnUnregister;

    public delegate void OnDeath(Butterfly butterfly);
    public static event OnDeath Died;

    public delegate void OnDying(Butterfly butterfly);
    public static event OnDying Dying;

    // External

    CameraDriver driver;
    
    Nest nest;
    ButterflowerManager mother;
    Quilt quilt;
    
    // Properties
    
    Wand[] wands;
    
    TrailRenderer trailRenderer;
    Renderer[] renderers;
    new Material material;
    MaterialPropertyBlock propertyBlock; 
    
    [Header("Base")]

    [SerializeField] Preset preset;
    public AGENT agent = AGENT.NULL;
    [SerializeField] Wand m_wand = null;
    
    [Header("Appearance")]
    
    [SerializeField] GameObject trailsPrefab;
                     GameObject trails = null;
                     ParticleSystem trails_ps;
                     
    Color baseColor = Color.white;
    
    Vector4 uv1, uv2;

    Vector3 color0, color1;

    [SerializeField] string shaderAnimationSpeedParam;

    [Header("Movement")]

    public Vector3 origin = Vector3.zero;
    public Vector3 m_positionRelativeToCamera;
    
    [SerializeField] Vector3 velocity = Vector3.zero;
    [SerializeField] float scale = 1f;
    
    [Header("State")]
    
    public State state = State.Hidden;

    float timeSinceAlive = 0f;
    float timeSinceDeath = 0f;

    #region Accessors
    
    public Wand wand => m_wand;

    public Vector3 positionRelativeToCamera
    {
        get
        {
            return m_positionRelativeToCamera;
        }
        set
        {
            m_positionRelativeToCamera = value;
        }
    }

    public Vector3 position
    {
        set
        {
            origin = value;
        }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public float Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    public State _State => state;
    
    #endregion

    #region Monobehavior callbacks

    void Awake()
    {
        trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    void Start()
    {
        Register();
        Init();
        Reset();
    }

    void OnDestroy(){
        StopCoroutine("RefreshWand");

        Unregister();
    }

    void Update()
    {
        if(state == State.Hidden)
        {
            transform.position = nest.transform.position; // Follow nest in space if hidden
            transform.localScale = Vector3.zero;

            return;
        }

        float dt = Time.deltaTime;

        timeSinceAlive += dt;

        EvaluateScaleFromState();

        float str = dt;
        if (state == State.Dying)
        {
            CreateTrails(); // Ensure trails are set
            FallToGround(timeSinceDeath);

            timeSinceDeath += dt;
        }

        if (state == State.Easing) 
        {
            CheckIfAlive();
        }
        else if (state == State.Alive) 
        {
            if (wand != null && wand.spells)
                WaitForWand(wand, str); // 0 - 1
        }

        AdjustAnimatorSpeed();

        var directionOfTravel = velocity * dt;
        trailRenderer.enabled = (state == State.Alive && velocity.magnitude > preset.velocityTrailThreshold);
    }

    #endregion

    #region Initialization and registration

    void Register()
    {
        if (OnRegister != null)
            OnRegister(this);
    }

    void Unregister()
    {
        if (OnUnregister != null)
            OnUnregister(this);
    }

    void Init()
    {
        driver = CameraDriver.Instance;
        nest = Nest.Instance;
        quilt = FindObjectOfType<Quilt>();
        mother = FindObjectOfType<ButterflowerManager>();
        wands = FindObjectsOfType<Wand>();

        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        material = renderers[0].sharedMaterial;
        material.SetFloat("_OverrideColorWeight", (quilt == null) ? 1f : 0f);

        StartCoroutine("RefreshWand");
    }
    
    #endregion
    
    #region Main operations

    public void Reset()
    {
        if (nest == null || nest.open)
        {
            state = State.Alive;
            transform.position = origin;

            StartCoroutine("TriggerDeathFromQuiltSpeed");
        }
        else
        {
            state = State.Hidden;
            transform.position = nest.transform.position;

            StopCoroutine("TriggerDeathFromQuiltSpeed");
        }

        velocity = Vector3.zero;
        transform.localScale = Vector3.zero;

        timeSinceAlive = timeSinceDeath = 0f;

        trails = null; // Ensure new trails created for every butterfly on respawn

        var prop = propertyBlock;
        prop.SetFloat("_Death", 0f);
        foreach (Renderer r in renderers) {
            r.SetPropertyBlock(prop);
        }
    }
    
    public void Release()
    {
        if (state == State.Hidden)
        {
            state = State.Easing;
            StartCoroutine("TriggerDeathFromQuiltSpeed");
        }
    }

    public void Kill()
    {
        onDeath();
    }

    #endregion

    #region State ops

    void CheckIfAlive()
    {
        Vector3 dir = (origin - transform.position);
        if (dir.magnitude < 1f)
            state = State.Alive; // Break out out Ease State if close enough to origin

        velocity = (dir);
    }

    void WaitForWand(Wand wand, float dt)
    {
        Vector3 target = wand.trajectory2d;
        Vector3 current = positionRelativeToCamera;

        Vector3 direction = (target - current);
        
        float radius = wand.radius;
        float dist = direction.magnitude;
        float offset = dist / radius;
        
        if (offset <= 1f) 
        {
            float minSpeed = preset.minimumWandSpeed;
            float maxSpeed = preset.maximumWandSpeed;
            
            var wandSpeedInterval = (wand.speed - minSpeed) / (maxSpeed - minSpeed);

            //Check if killing butterfly, SPEED TOO GREAT
            if (wandSpeedInterval > 1f)
                onDeath(AGENT.User); // Flag butterfly for death
        }
    }

    #endregion

    #region Scaling
    
    void EvaluateScaleFromState()
    {
        float scale = preset.scale;
        if (timeSinceAlive <= preset.timeToGrow)
            GrowOverTime(ref scale);
        else 
        {
            if(wand != null) 
                GrowWithWand(wand, ref scale);
            if (nest != null && nest.energy > 0f)
                GrowWithNest(ref scale);
        }
        transform.localScale = Vector3.one * scale;
    }

    void GrowOverTime(ref float scale)
    {
        float interval = timeSinceAlive / preset.timeToGrow;
        scale = Mathf.Lerp(0f, preset.scale, Mathf.Pow(interval, 2f));
    }

    void GrowWithWand(Wand wand, ref float scale)
    {
        Vector3 dir = (wand.position3d - positionRelativeToCamera);
        float magnitude = Mathf.Clamp01(1f - dir.magnitude / (wand.radius*6f)) + ((nest == null)?0f:nest.energy*preset.energyGrowth);

        scale *= (1f + Mathf.Pow(magnitude, 2f));
    }

    void GrowWithNest(ref float scale)
    {
        //float magnitude = nest.energy;
        //scale *= (1f + magnitude*2f);
    }

    #endregion

    #region Trails
    
    void CreateTrails()
    {
        if (trails != null)
            return;

        trails = Instantiate(trailsPrefab, transform);
        trails.transform.localPosition = Vector3.zero;
        trails.transform.localScale = Vector3.one * preset.trailsSize;

        trails_ps = trails.GetComponent<ParticleSystem>();

        var main = trails_ps.main;
        main.loop = true;

        trails_ps.Play();
    }

    void DestroyTrails()
    {
        if (trails == null)
            return;

        var main = trails_ps.main;
        main.loop = false;

        trails = null;
    }
    
    #endregion
    
    #region Animation
    
    void AdjustAnimatorSpeed()
    {
        float speed = (state == State.Dying) ? preset.maxAnimationSpeed : preset.minAnimationSpeed;
        float lastSpeed = propertyBlock.GetFloat(shaderAnimationSpeedParam);
        
        float currentSpeed = (speed - lastSpeed) * Time.deltaTime;
        propertyBlock.SetFloat(shaderAnimationSpeedParam, currentSpeed);
    }
    
    #endregion

    #region Wands

    IEnumerator RefreshWand()
    {
        Wand temp = null;

        while (true) {
            temp = null;

            if (wands.Length == 1) temp = wands[0];
            if (state == State.Alive && wands.Length > 1) {
                
                float maxDistance = Mathf.Infinity;
                
                for (int i = 0; i < wands.Length; i++) 
                {
                    var w = wands[i];
                    float dist = 0f;

                    if (w.global) 
                    {
                        dist = (w.position2d - positionRelativeToCamera).magnitude;
                    }
                    else {
                        dist = (w.position3d - transform.position).magnitude;
                    }

                    
                    if (dist <= w.radius && dist <= maxDistance) 
                    {
                        temp = w;
                        maxDistance = dist;
                    }
                }
            }

            m_wand = temp;

            yield return new WaitForSeconds(.33f);
        }
    }

	#endregion

	#region Dying
    
    void Die()
    {
        trails.transform.parent = null;
        DestroyTrails();

        state = State.Hidden;
        if (Died != null)
            Died(this);
    }

    void FallToGround(float timeSinceDeath)
    {
        float d = (timeSinceDeath - preset.deathColorDelay) / preset.deathTransitionTime; // Go full white --> .167 = time to go white
        if (d >= 0f && d <= 1f)
        {
            var prop = propertyBlock;
            prop.SetFloat("_Death", Mathf.Clamp01(d));
            foreach (Renderer r in renderers) {
                r.SetPropertyBlock(prop);
            }
        }
        
        if (timeSinceDeath > preset.timeDead) Die();
    }
    
    IEnumerator TriggerDeathFromQuiltSpeed()
    {
        while (state == State.Alive || state == State.Easing) {
            yield return new WaitForSeconds(preset.colorRefresh);

            float prob = preset.deathProbabilityCurve.Evaluate((quilt == null) ? 0f : quilt.speedInterval);

            if (Random.Range(0f, 1f) < prob) {
                onDeath(AGENT.Inhabitants);
            }
        }
    }

	void onDeath(AGENT agent = AGENT.World)
    {
        bool flag = (state != State.Dying);

        state = State.Dying;
        if (flag && Dying != null) {
            this.agent = agent;
            Dying(this);
        }
    }

	#endregion
}
