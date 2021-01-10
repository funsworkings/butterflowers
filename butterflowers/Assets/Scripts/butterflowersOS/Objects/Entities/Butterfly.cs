using butterflowersOS.Core;
using UnityEngine;
using Preset = butterflowersOS.Presets.ButterflyPreset;

namespace butterflowersOS.Objects.Entities
{
    public class Butterfly : MonoBehaviour
    {
        #region Internal
    
        public enum State
        {
            Hidden = 0,
            Easing = 1,
            Alive = 2,
            Dying = 3
        }
    
        #endregion
    
        // Events
    
        public static System.Action<Butterfly> OnRegister, OnUnregister;

        // External
    
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

        #region Accessors

        public Vector3 position
        {
            set
            {
                origin = value;
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
            /*
        if (state == State.Dying)
        {
            //CreateTrails(); // Ensure trails are set
            ContinueDying(timeInState);
        }

        AdjustAnimatorSpeed();
        trailRenderer.enabled = (state == State.Alive && velocity.magnitude > preset.velocityTrailThreshold);
        */
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
            quilt = FindObjectOfType<Quilt>();
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
            trails = null; // Ensure new trails created for every butterfly on respawn
        
            propertyBlock.SetFloat("_Death", 0f);
            propertyBlock.SetFloat("_TimeOffset", Random.Range(0f, 1f));
            propertyBlock.SetFloat("_Speed", Random.Range(0.5f, 1.5f));
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
            /*
        float speed = (state == State.Dying) ? preset.maxAnimationSpeed : preset.minAnimationSpeed;
        float lastSpeed = propertyBlock.GetFloat(shaderAnimationSpeedParam);
        
        float currentSpeed = (speed - lastSpeed) * Time.deltaTime;
        propertyBlock.SetFloat(shaderAnimationSpeedParam, currentSpeed);
        */
        }
    
        #endregion

        #region Death

        void ContinueDying(float timeSinceDeath)
        {
            float d = (timeSinceDeath - preset.deathColorDelay) / preset.deathTransitionTime; // Go full white --> .167 = time to go white
            if (d >= 0f && d <= 1f)
            {
                propertyBlock.SetFloat("_Death", Mathf.Clamp01(d));
                propertyBlock.SetFloat("_TimeOffset", Random.Range(0f, 1f));
                propertyBlock.SetFloat("_Speed", Random.Range(0.5f, 1.5f));
                foreach (Renderer r in renderers) {
                    r.SetPropertyBlock(propertyBlock);
                }
            }
        }

        #endregion
    }
}
