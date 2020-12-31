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

    float timeInState = 0f;

    #region Accessors

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

    public float TimeInState
    {
        get { return timeInState; }
        set { timeInState = value; }
    }

    public State _State
    {
        get { return state; }
        set
        {
            bool flag_change = (state != value);
            if (flag_change) 
            {
                if (value == State.Dying) 
                {
                    if (Dying != null) 
                        Dying(this);
                }

                timeInState = 0f;
            }

            state = value;
        }
    }
    
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

    void OnDestroy()
    {
        Unregister();
    }

    void Update()
    {
        timeInState += Time.deltaTime;
        if (state == State.Hidden) return;

        if (state == State.Dying)
        {
            //CreateTrails(); // Ensure trails are set
            ContinueDying(timeInState);
        }

        //AdjustAnimatorSpeed();
        //trailRenderer.enabled = (state == State.Alive && velocity.magnitude > preset.velocityTrailThreshold);
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
    }
    
    #endregion
    
    #region Main operations

    public void Reset()
    {
        if (nest == null || nest.open)
        {
            state = State.Alive;
            transform.position = origin;
        }
        else
        {
            state = State.Hidden;
            transform.position = nest.transform.position;
        }

        velocity = Vector3.zero;
        transform.localScale = Vector3.zero;

        timeInState = 0f;

        trails = null; // Ensure new trails created for every butterfly on respawn
        
        propertyBlock.SetFloat("_Death", 0f);
        foreach (Renderer r in renderers) {
            r.SetPropertyBlock(propertyBlock);
        }
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

    #region Death
    
    void Die()
    {
        //trails.transform.parent = null;
        //DestroyTrails();

        state = State.Hidden;
        if (Died != null)
            Died(this);
    }

    void ContinueDying(float timeSinceDeath)
    {
        float d = (timeSinceDeath - preset.deathColorDelay) / preset.deathTransitionTime; // Go full white --> .167 = time to go white
        if (d >= 0f && d <= 1f)
        {
            propertyBlock.SetFloat("_Death", Mathf.Clamp01(d));
            foreach (Renderer r in renderers) {
                r.SetPropertyBlock(propertyBlock);
            }
        }
        
        if (timeSinceDeath > preset.timeDead) 
            Die();
    }

    #endregion
}
