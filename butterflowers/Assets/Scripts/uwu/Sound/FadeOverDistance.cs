﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class FadeOverDistance : MonoBehaviour
{
    [SerializeField] Transform origin, target;
    [SerializeField] bool x = true, y = true, z = true;

    [SerializeField] float minDistance = 0f, maxDistance = 1f;
    [SerializeField] float distance = 0f;
    [SerializeField] AnimationCurve falloff;

    [SerializeField] float minVolume = 0f, maxVolume = 1f;
    [SerializeField] float volume = 0f;

    [SerializeField] bool inverse = false;

    new AudioSource audio;
        SphereCollider radius;

    void Awake() {
        audio = GetComponent<AudioSource>();    
        radius = GetComponent<SphereCollider>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        audio.spatialBlend = 0f; // Ensure 2D sound for audio source on start, will be controlled by script
        radius.isTrigger = true; // Ensure collisions aren't triggered

        if(origin == null) origin = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(target == null) return;

        Vector3 target_position = target.position;
        if(!x) target_position.x = origin.position.x;
        if(!y) target_position.y = origin.position.y;
        if(!z) target_position.z = origin.position.z;

        maxDistance = radius.radius;

        Fade(target_position);
    }

    [SerializeField] float d = 0f;

    void Fade(Vector3 position){
        distance = Vector3.Distance(origin.position, position);
            d = distance.Remap(minDistance, maxDistance, 0f, 1f);

        if(!inverse) d = (1f - d);

        volume = (falloff.Evaluate(d)).Remap(minVolume, maxVolume);
        audio.volume = volume;
    }
}
