using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Settings {

    [CreateAssetMenu(fileName = "New Butterfly Preset", menuName = "Settings/Butterfly Preset", order = 52)]
    public class ButterflyPreset : ScriptableObject {
        public float maxSpeed;

        public float noiseAmount;
        public float noiseSize;

        public float moveAmount;
        public float attraction;

        public float gravity = .98f;
        public float descentTime;
        public float trailsSize;
        public float colorRefresh;

        public float minLifetime, maxLifetime; 

        public float wandRadius;
        public float wandRepelSpeed;

        [Tooltip("Decay measured based on speed / max speed")]
        public AnimationCurve energyDecayCurve;
        [Tooltip("Growth measured based on energy / 1.0")]
        public AnimationCurve energyGrowthCurve;

        public AnimationCurve attractionCurve;

        public float centerStrength;
        public float minCenterDistance;

        public float energyGrowth, energyDecay;
        public float recoveryTime;

        public float minAnimationSpeed = 1f, maxAnimationSpeed = 2f;
        public float timeDead = 1f;

        public float scale = .167f;
        public float timeToGrow = 1f;
    }

}