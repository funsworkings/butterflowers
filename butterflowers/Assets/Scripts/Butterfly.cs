using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Settings;
using Preset = Settings.ButterflyPreset;

public class Butterfly : MonoBehaviour
{
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

    [SerializeField] float energy = 1f;

    public bool alive = true;
    public bool dying = false;

        float timeSinceDeath = 0f;
        float lifetime = 0f;
        float life = 0f;

    Vector4 uv1, uv2;

    // Start is called before the first frame update
    void Start()
    {
        driver = CameraDriver.Instance;

        mother = FindObjectOfType<Mother>();
        wand = FindObjectOfType<Wand>();

        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();

     /*  var viewport = driver.ConvertToViewport(transform.position);
        uv1 = uv2 = new Vector4(viewport.x, viewport.y, 0f, 0f);

        foreach(Renderer r in renderers){
            r.material.SetVector("_UV1", uv1);
            r.material.SetVector("_UV2", uv2);
        }*/

        lifetime = Random.Range(preset.minLifetime, preset.maxLifetime);
        StartCoroutine("UpdateAttentuationFromTexture");
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if(alive){
            float str = dt;
            if(dying){
                if(trails == null){
                    trails = Instantiate(trailsPrefab, transform);
                        trails.transform.localPosition = Vector3.zero;
                        trails.transform.localScale = Vector3.one * preset.trailsSize;

                    trails_ps = trails.GetComponent<ParticleSystem>();
                    var main = trails_ps.main;
                        main.loop = true;

                    trails_ps.Play();
                }

                float duration = preset.descentTime;
                str *= (1f - Mathf.Clamp01((timeSinceDeath) / duration));

                timeSinceDeath += dt;
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


       /* var viewport = driver.ConvertToViewport(transform.position);
        uv2 = new Vector4(viewport.x, viewport.y, 0f, 0f);
        foreach(Renderer r in renderers){
            r.material.SetVector("_UV2", uv2);
        }*/
    }

    void LateUpdate() {
        float dt = Time.deltaTime;

        if(!dying)
            ClampVelocity();
            
        transform.position += velocity * dt;
    }

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

    void Die(){
        trails.transform.parent = null;

        var main = trails_ps.main;
            main.loop = false;

        var emission = trails_ps.emission;
        var burst = new ParticleSystem.Burst(Mathf.Repeat(trails_ps.time + .01f, main.duration), 100);
            emission.SetBursts(new ParticleSystem.Burst[]{ burst });

        alive = false;
        dying = false;

        GameObject.Destroy(gameObject);
    }


    IEnumerator UpdateAttentuationFromTexture(){
        while(alive && !dying){
            var viewport = driver.ConvertToViewport(transform.position);

            value = mother.GetColorFromCanvas(viewport);
            var rgb = new Vector3(value.r, value.g, value.b);

           /* uv1 = uv2;
            foreach(Renderer r in renderers){
                r.material.SetVector("_UV1", uv1);
            }*/

            if(rgb.magnitude < .33f)
                dying = true;

            yield return new WaitForSeconds(preset.colorRefresh);
        }

    }
}
