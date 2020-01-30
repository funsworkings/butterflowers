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

        public float minDistanceFromWand;
        public float maxDistanceFromWand;

        [Tooltip("Decay measured based on speed / max speed")]
        public AnimationCurve energyDecayCurve;
        [Tooltip("Growth measured based on energy / 1.0")]
        public AnimationCurve energyGrowthCurve;

        public float energyGrowth, energyDecay;
        public float recoveryTime;
    }

}