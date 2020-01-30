using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settingss : MonoBehaviour
{
    [Header("Butterflies")]
        public float b_moveSpeed = 3f;
        public float b_attraction = 1f;

        public float b_minDistanceFromWand = 1f;
        public float b_maxDistanceFromWand = 10f;

        public float b_noiseAmount = 10f;
        public float b_noiseSize = 10f;
}
