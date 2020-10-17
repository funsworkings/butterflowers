using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Settings {

    [CreateAssetMenu(fileName = "New Butterfly Preset", menuName = "Settings/Presets/Butterfly", order = 52)]
    public class ButterflyPreset : ScriptableObject {
        public float maxSpeed;

        public float noiseAmount;
        public float noiseSize;
        
        public float attraction;
        public float follow;

        public float gravity = .98f;
        public float descentTime;
        public float trailsSize;
        public float colorRefresh;

        public float minLifetime, maxLifetime; 
        
        [FormerlySerializedAs("wandRepelSpeed")] public float minimumWandSpeed;
        public float maximumWandSpeed;

        [FormerlySerializedAs("attractionCurve")] public AnimationCurve distanceAttractionCurve;
        public AnimationCurve speedAttractionCurve;

        public float velocityDampening = 1f;
        public float velocityTrailThreshold = 1f;

        public float centerStrength;
        public float minCenterDistance;

        public float energyGrowth;

        public float minAnimationSpeed = 1f, maxAnimationSpeed = 2f;
        public float timeDead = 1f, deathColorDelay = 1f, deathTransitionTime = .33f;

        public float scale = .167f;
        public float timeToGrow = 1f;
        
        public AnimationCurve deathProbabilityCurve;
    }

}