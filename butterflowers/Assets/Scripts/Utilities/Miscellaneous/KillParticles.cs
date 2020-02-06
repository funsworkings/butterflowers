using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class KillParticles : MonoBehaviour
{
    ParticleSystem ps;

    bool played = false;

    void Awake() {
        ps = GetComponent<ParticleSystem>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(played){
            if(!ps.isPlaying){
                if(ps.particleCount <= 0)
                    Kill();
            }
        }

        if(ps.isPlaying)
            played = true;
    }

    void Kill(){
        GameObject.Destroy(gameObject);
    }
}
