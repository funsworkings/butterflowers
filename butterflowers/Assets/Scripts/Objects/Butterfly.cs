using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Settings;
using Preset = Settings.ButterflyPreset;

public class Butterfly : MonoBehaviour
{
    public static System.Action<Butterfly> OnRegister, OnUnregister;

    public delegate void OnDeath(Butterfly butterfly);
    public static event OnDeath Died;

    public delegate void OnDying(Butterfly butterfly);
    public static event OnDying Dying;

    CameraDriver driver;

    Nest nest;
    MotherOfButterflies mother;
    Quilt quilt;
    Wand[] wands;

    Animator animator;
    Renderer[] renderers;
    MaterialPropertyBlock propertyBlock; 

    [SerializeField] Preset preset;
    [SerializeField] GameObject trailsPrefab;
                     GameObject trails = null;
                     ParticleSystem trails_ps;

    [SerializeField] Wand m_wand = null;
    public Wand wand => m_wand;

    public Vector3 positionRelativeToCamera
    {
        get
        {
            return driver.ConvertToScreen(transform.position);
        }
    }

    public Vector3 position
    {
        set
        {
            origin = value;
        }
    }

    public Vector3 origin = Vector3.zero, spawn = Vector3.zero;

    Vector3 velocity = Vector3.zero;
    Color value = Color.white;

    public float depth => driver.camera.transform.InverseTransformPoint(transform.position).z;

    public enum State
    {
        Hidden,
        Easing,
        Alive,
        Dying
    }
    public State state = State.Hidden;

    float timeSinceAlive = 0f;
    float timeSinceDeath = 0f;
    float lifetime = 0f;

    Vector4 uv1, uv2;

    [SerializeField] Color final;

    Vector3 color0, color1;
    public float colorSpeed = 0f;
    bool colorized = false;

    #region Monobehavior callbacks

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

        float scale = preset.scale;
        if (timeSinceAlive <= preset.timeToGrow)
            GrowOverTime(ref scale);
        else 
        {
            if(wand != null) 
                GrowWithWand(wand, ref scale);
            if (nest.energy > 0f)
                GrowWithNest(ref scale);
        }
        transform.localScale = Vector3.one * scale;

        float str = dt;
        if (state == State.Dying)
        {
            CreateTrails();

            float duration = preset.descentTime;
            str *= (1f - Mathf.Clamp01((timeSinceDeath) / duration));

            float d = (timeSinceDeath - preset.deathColorDelay) / preset.deathTransitionTime; // Go full white --> .167 = time to go white
            if (d >= 0f && d <= 1f)
            {
                var prop = propertyBlock;
                prop.SetFloat("_Death", Mathf.Clamp01(d));
                foreach (Renderer r in renderers) {
                    r.SetPropertyBlock(prop);
                }
            }

            MoveTowardsGround(timeSinceDeath);
            if (positionRelativeToCamera.y <= 64f && timeSinceDeath > preset.timeDead)
                Die();

            timeSinceDeath += dt;
        }

        if (state == State.Easing)
            MoveTowardsSpawn(str);
        else if(state == State.Alive)
        {
            if(wand != null && wand.spells) MoveWithWand(wand, str);
  
            MoveTowardsCenter(str);
            MoveWithNoise(str);
        }

        AdjustAnimatorSpeed();

        if (state != State.Dying)
            ClampVelocity();

        transform.position += velocity * dt;
    }

    #endregion

    #region Operations

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
        mother = FindObjectOfType<MotherOfButterflies>();
        wands = FindObjectsOfType<Wand>();
        animator = GetComponentInChildren<Animator>();

        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        StartCoroutine("RefreshWand");
    }

    public void Reset()
    {
        if (nest.open)
        {
            state = State.Alive;
            transform.position = origin;

            StartCoroutine("UpdateAttentuationFromTexture");
        }
        else
        {
            state = State.Hidden;
            transform.position = nest.transform.position;

            StopCoroutine("UpdateAttentuationFromTexture");
        }

        velocity = Vector3.zero;
        transform.localScale = Vector3.zero;

        timeSinceAlive = timeSinceDeath = 0f;
        lifetime = Random.Range(preset.minLifetime, preset.maxLifetime);

        trails = null; // Ensure new trails created for every butterfly on respawn

        var prop = propertyBlock;
        prop.SetFloat("_Death", 0f);
        foreach (Renderer r in renderers) {
            r.SetPropertyBlock(prop);
        }

        colorized = false;
    }

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
     
    void AdjustAnimatorSpeed()
    {
        float speed = (state == State.Dying) ? preset.maxAnimationSpeed : preset.minAnimationSpeed;
        animator.speed += (speed - animator.speed) * Time.deltaTime;
    }

    void Die()
    {
        trails.transform.parent = null;
        DestroyTrails();
        /*var main = trails_ps.main;
            main.loop = false;

        var emission = trails_ps.emission;
        var burst = new ParticleSystem.Burst(Mathf.Repeat(trails_ps.time + .01f, main.duration), 100);
            emission.SetBursts(new ParticleSystem.Burst[]{ burst });*/

        state = State.Hidden;
        if (Died != null)
            Died(this);
    }

    public void Release()
    {
        if (state == State.Hidden)
        {
            state = State.Easing;
            StartCoroutine("UpdateAttentuationFromTexture");
        }
    }

    public void Kill()
    {
        onDeath();
    }

    #endregion

    #region Movement

    void ClampVelocity(){
        float speed = velocity.magnitude;
              speed = Mathf.Min(speed, preset.maxSpeed);

        velocity = (speed * velocity.normalized);
    }

    void MoveTowardsSpawn(float dt)
    {
        Vector3 dir = (origin - transform.position);
        if (dir.magnitude < 1f)
            state = State.Alive; // Break out out Ease State if close enough to origin

        transform.position = Vector3.Lerp(transform.position, origin, .5f * dt);
    }

    float MoveWithNoise(float dt){
        Vector2 screen = positionRelativeToCamera * preset.noiseSize;
        float noise = Mathf.PerlinNoise(screen.x, screen.y);

        Vector2 dir = Random.insideUnitCircle;
        float speed = preset.noiseAmount * noise;

        transform.position += (driver.MoveRelativeToCamera(dir) * speed * dt);

       return speed;
    }

    void MoveWithWand(Wand wand, float dt){
        Vector3 target = wand.position;
        Vector3 current = positionRelativeToCamera;

        Vector3 direction = (target - current);

        float radius = preset.wandRadius;
        float dist = direction.magnitude;
        float offset = (radius - dist);
        float offset_int = offset / radius;

        if (offset_int > 0f)
        {
            float magnitude = preset.attractionCurve.Evaluate(Mathf.Clamp01(offset_int));

            //Check if killing butterfly
            if (wand.speed >= preset.wandRepelSpeed)
            {
                onDeath(); // Flag butterfly for death
                magnitude *= -1f;
            }

            velocity += driver.MoveRelativeToCamera(direction.normalized) * magnitude * preset.attraction * dt;
           // velocity += driver.MoveRelativeToCamera(wand.velocity.normalized) * Mathf.Pow((1f - magnitude), 2f) * preset.follow * dt;
        }
    }

    void MoveTowardsCenter(float dt){
        Vector3 viewPosition = driver.ConvertToViewport(transform.position);
        Vector3 center = (Vector3.one * .5f);

        Vector3 direction = driver.MoveRelativeToCamera(center - viewPosition); // Grab vector to center of screen

        float distanceFromCenter = direction.magnitude;
        float magnitude = Mathf.Max(0f, (distanceFromCenter - preset.minCenterDistance));

        velocity += (direction.normalized) * preset.centerStrength * Mathf.Pow(magnitude, 2f);
    }

    void MoveTowardsGround(float t){
        velocity += driver.MoveRelativeToCamera(Vector3.down) * Mathf.Pow(t, 2f) * preset.gravity;
    }

    #endregion

    #region Growth

    void GrowOverTime(ref float scale)
    {
        float interval = timeSinceAlive / preset.timeToGrow;
        scale = Mathf.Lerp(0f, preset.scale, Mathf.Pow(interval, 2f));
    }

    void GrowWithWand(Wand wand, ref float scale)
    {
        Vector3 dir = (wand.position - positionRelativeToCamera);
        float magnitude = Mathf.Clamp01(1f - dir.magnitude / preset.wandRadius) + nest.energy*2f;

        scale *= (1f + Mathf.Pow(magnitude, 2f));
    }

    void GrowWithNest(ref float scale)
    {
        //float magnitude = nest.energy;
        //scale *= (1f + magnitude*2f);
    }

    #endregion

    #region Appearance

    void UpdateMaterials() {
        float death = (state == State.Dying) ? Mathf.Clamp01((timeSinceDeath - preset.deathColorDelay) / preset.deathTransitionTime) : 0f;

        var prop = propertyBlock;
        prop.SetFloat("_Death", death);
        foreach (Renderer r in renderers) {
            r.SetPropertyBlock(prop);
        }
    }


    IEnumerator UpdateAttentuationFromTexture(){
        while(state == State.Alive || state == State.Easing){
            yield return new WaitForSeconds(preset.colorRefresh);

            float prob = preset.deathProbabilityCurve.Evaluate(quilt.speedInterval);

            if (Random.Range(0f, 1f) < prob) 
            {
                onDeath();
                final = value;
            }

            /*
            var viewport = driver.ConvertToViewport(transform.position);

            value = quilt.GetColorFromCanvas(viewport);
            var rgb = new Vector3(value.r, value.g, value.b);

            if (!colorized)
            {
                color1 = color0 = rgb;
                colorSpeed = 0f;
                colorized = true;
            }
            else
            {
                color1 = rgb;
                colorSpeed = (color1 - color0).magnitude / 1.8f; // Divide by rough (1,1,1) magnitude
                color0 = color1;
            }

            if (colorSpeed > preset.maximumColorSpeed)
            {
                state = State.Dying;
                final = value;
            }*/

            
        }

    }

    #endregion

    #region Wands

    IEnumerator RefreshWand()
    {
        Wand temp = null;

        while (true) {
            temp = null;

            if (wands.Length == 1) temp = wands[0];
            if (state == State.Alive && wands.Length > 1) 
            {
                float d = preset.wandRadius;
                
                for (int i = 0; i < wands.Length; i++) 
                {
                    var w = wands[i];
                    float dist = (w.position - positionRelativeToCamera).magnitude;
                    if (dist <= d) 
                    {
                        temp = w;
                        d = dist;
                    }
                }
            }

            m_wand = temp;

            yield return new WaitForSeconds(.33f);
        }
    }

	#endregion

	#region Internal callbacks

	void onDeath()
    {
        bool flag = (state != State.Dying);

        state = State.Dying;
        if (flag && Dying != null)
            Dying(this);
    }

	#endregion
}
