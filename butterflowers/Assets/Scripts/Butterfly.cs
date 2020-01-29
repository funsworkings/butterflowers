using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butterfly : MonoBehaviour
{
    new Camera camera;
    Wand wand;

    float moveSpeed = 1f;
    float maxDistanceFromWand = 3f;

    float noiseAmount = 10f;
    float noiseSize = 10f;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        wand = FindObjectOfType<Wand>();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        MoveWithNoise(dt);
        MoveTowardsWand(dt);
    }

    void MoveWithNoise(float dt){
        Vector2 screen = camera.WorldToScreenPoint(transform.position) * noiseSize;
        float noise = Mathf.PerlinNoise(screen.x, screen.y) * noiseAmount;

        Vector3 dir = Random.insideUnitSphere;

        transform.position += (dir * noise * moveSpeed * dt);
    }

    void MoveTowardsWand(float dt){
        if(wand == null)
            return;

        Vector3 target = wand.position;
        Vector3 dir = (target - transform.position);
            dir.z = 0f;

        float distance = dir.magnitude;
        float magnitude = Mathf.Pow(1f - Mathf.Clamp01(distance / maxDistanceFromWand), 2f);
            transform.position += (dir.normalized * dt * moveSpeed * magnitude);
    }
}
