using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Settings;
using Preset = Settings.ButterflyPreset;

public class Butterfly : MonoBehaviour
{
    public delegate void OnDeath(Butterfly butterfly);
    public static event OnDeath Died;

    CameraDriver driver;

    Mother mother;
    Wand wand;

    Animator animator;
    Renderer[] renderers;

    [SerializeField] Preset preset;
    [SerializeField] GameObject trailsPrefab;
                     GameObject trails = null;
                     ParticleSystem trails_ps;

    public Vector3 position { get{ return driver.ConvertToScreen(transform.position); } }

    Vector3 velocity = Vector3.zero;
    Color value = Color.white;

    public bool alive = true;
    public bool dying = false;

        float timeSinceDeath = 0f;
        float delayToDeath = .67f;
        float lifetime = 0f;

    Vector4 uv1, uv2;

    [SerializeField] Color final;

    void Start()
    {
        if (driver == null) driver = CameraDriver.Instance;

        if (mother == null) mother = FindObjectOfType<Mother>();
        if (wand == null) wand = FindObjectOfType<Wand>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();

        Reset();

        StartCoroutine("UpdateAttentuationFromTexture");
    }

    public void Reset() {
        alive = true;
        dying = false;

        velocity = Vector3.zero;

        timeSinceDeath = 0f;
        lifetime = Random.Range(preset.minLifetime, preset.maxLifetime);

        trails = null; // Ensure new trails created for every butterfly on respawn

        foreach (Renderer r in renderers)
            r.material.SetFloat("_Death", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if(alive){
            float str = dt;
            if(dying){
                float duration = preset.descentTime;
                str *= (1f - Mathf.Clamp01((timeSinceDeath) / duration));


                if (trails == null)
                {
                    trails = Instantiate(trailsPrefab, transform);
                    trails.transform.localPosition = Vector3.zero;
                    trails.transform.localScale = Vector3.one * preset.trailsSize;

                    trails_ps = trails.GetComponent<ParticleSystem>();

                    var main = trails_ps.main;
                        main.loop = true;

                    trails_ps.Play();
                }

                timeSinceDeath += dt;

                float d = (timeSinceDeath - delayToDeath) / .167f; // Go full white --> .167 = time to go white
                if (d >= 0f && d <= 1f)
                {
                    foreach (Renderer r in renderers)
                        r.material.SetFloat("_Death", Mathf.Clamp01(d));
                }

                MoveTowardsGround(timeSinceDeath);

                if(position.y <= 64f)
                    Die();
            }

            MoveWithWand(str);
            MoveTowardsCenter(str);
            MoveWithNoise(str);
        }

        float speed = (dying)? preset.maxAnimationSpeed:preset.minAnimationSpeed;
        animator.speed += (speed - animator.speed)*dt;


        if (!dying)
            ClampVelocity();

        transform.position += velocity * dt;
    }

    #region Movement

    void ClampVelocity(){
        float speed = velocity.magnitude;
              speed = Mathf.Min(speed, preset.maxSpeed);

        velocity = (speed * velocity.normalized);
    }

    float MoveWithNoise(float dt){
        Vector2 screen = position * preset.noiseSize;
        float noise = Mathf.PerlinNoise(screen.x, screen.y);

        Vector2 dir = Random.insideUnitCircle;
        float speed = preset.noiseAmount * noise;

        transform.position += (driver.MoveRelativeToCamera(dir) * speed * dt);

       return speed;
    }

    void MoveWithWand(float dt){
        Vector3 target = wand.position;
        Vector3 current = position;

        Vector3 direction = (target - current);

        float radius = preset.wandRadius;
        float magnitude = preset.attractionCurve.Evaluate(Mathf.Clamp(radius - direction.magnitude, 0f, radius) / radius);

        if(wand.speed >= preset.wandRepelSpeed) {
            if(magnitude > 0f)
                dying = true; // Flag butterfly for death

            magnitude *= -1f;
        }
        
        velocity += driver.MoveRelativeToCamera(direction.normalized) * magnitude * preset.attraction * dt;
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

    void Die(){
        trails.transform.parent = null;

        var main = trails_ps.main;
            main.loop = false;

        var emission = trails_ps.emission;
        var burst = new ParticleSystem.Burst(Mathf.Repeat(trails_ps.time + .01f, main.duration), 100);
            emission.SetBursts(new ParticleSystem.Burst[]{ burst });

        alive = false;
        dying = false;

        if (Died != null)
            Died(this);
    }

    #region Appearance

    void UpdateMaterials() {
        float death = (dying) ? Mathf.Clamp01((timeSinceDeath - delayToDeath) / .33f) : 0f;

        foreach (Renderer r in renderers)
            r.material.SetFloat("_Death", death);
    }


    IEnumerator UpdateAttentuationFromTexture(){
        while(alive && !dying){
            var viewport = driver.ConvertToViewport(transform.position);

            value = mother.GetColorFromCanvas(viewport);
            var rgb = new Vector3(value.r, value.g, value.b);

            if (rgb.magnitude < .33f)
            {
                dying = true;
                final = value;
            }

            yield return new WaitForSeconds(preset.colorRefresh);
        }

    }

    #endregion
}
