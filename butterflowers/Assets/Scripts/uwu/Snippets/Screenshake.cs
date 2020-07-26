using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

[RequireComponent(typeof(Camera))]
public class Screenshake : MonoBehaviour
{
    new Camera camera;
    CinemachineBrain brain;

    [SerializeField] float radius = 1f;
    [SerializeField] float dampening = 1f;

    Vector3 origin = Vector3.zero;

    bool active = false;
    float shake = 0f;

    public bool inprogress => shake > 0f;

    void Awake()
    {
        camera = GetComponent<Camera>();
        brain = GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (inprogress) 
        {
            shake -= Time.deltaTime * dampening;
            if (shake < 0f) 
            {
                shake = 0f;
                Dispose();
            }
            else
                onShake();
        }
    }

    public void Shake(float strength = 1f)
    {
        if (inprogress) return;
        shake = strength;

        if (camera == null) return;
        if (brain != null) brain.enabled = false;

        var transform = camera.transform;
        origin = transform.position;

        onShake();
    }

    void onShake()
    {
        Vector3 direction = Random.insideUnitSphere;

        float adjustment = 1f / direction.magnitude;
        direction *= (adjustment * radius * shake);

        transform.position = (origin + direction);
    }

    void Dispose()
    {
        if (camera == null) return;

        camera.transform.position = origin;
        if (brain != null) brain.enabled = true;
    }
}
