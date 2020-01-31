using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Settings;
using Preset = Settings.ButterflyPreset;

public class Butterfly : MonoBehaviour
{
    new Camera camera;
    Wand wand;

    Animator animator;

    [SerializeField] Preset preset;
    

    public Vector3 position { get{ return camera.WorldToScreenPoint(transform.position); } }


    Vector3 velocity = Vector3.zero;
    [SerializeField] float energy = 1f;

    float timeSinceRecover = 0f;
    bool recovering = false;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        wand = FindObjectOfType<Wand>();

        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        //Decay(noise, dt);

        
        if(wand != null){
            float attract = MoveTowardsWand(dt);
            Decay(attract, dt);
        }


        float noise = MoveWithNoise(dt);
        Grow(dt);


        animator.speed = (preset.maxAnimationSpeed - preset.minAnimationSpeed) * energy + preset.minAnimationSpeed;
    }

    float MoveWithNoise(float dt){
        Vector2 screen = position * preset.noiseSize;
        float noise = Mathf.PerlinNoise(screen.x, screen.y);

        Vector3 dir = Random.insideUnitSphere;
        float speed = preset.noiseAmount * noise;

        transform.position += (dir * speed * dt);

       return speed;
    }

    float MoveTowardsWand(float dt){
        Vector3 target = wand.position;
        Vector3 dir = (target - position);

        float distance = dir.magnitude;

        float a = Mathf.Clamp01((distance - preset.minDistanceFromWand) / (preset.maxDistanceFromWand - preset.minDistanceFromWand));
        float magnitude = preset.attraction * preset.attractionCurve.Evaluate(a);

        float speed = preset.moveAmount * magnitude;
        Vector3 move = camera.transform.right * dir.x + camera.transform.up * dir.y;

        transform.position += (move * speed * energy * dt);

        return speed * dt;
    }

    void Decay(float speed, float dt){
        float decay = preset.energyDecay * preset.energyDecayCurve.Evaluate(Mathf.Clamp01(speed / preset.maxSpeed));
        energy = Mathf.Clamp01(energy - decay*dt);
    }

    void Grow(float dt){
        float grow = preset.energyGrowth * preset.energyGrowthCurve.Evaluate(1f - Mathf.Clamp01(energy / 1f));
        if(energy <= 0f)
            grow = preset.energyGrowth;

        energy = Mathf.Clamp01(energy + grow*dt);
    }
}
