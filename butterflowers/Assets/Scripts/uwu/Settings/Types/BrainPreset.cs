using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wizard;

[CreateAssetMenu(fileName = "New Brain Preset", menuName = "Settings/Presets/Brain", order = 52)]
public class BrainPreset : ScriptableObject
{
    [Header("Time")]
        public float daysUntilEnvironmentKnowledge = 7f;
        public float daysUntilFileKnowledge = 7f;

    [Header("Multipliers")]
        public float nestLearningMultiplier = 1f;

    [Header("Stance")]
        public float environmentWeight = 1f;
        public float filesWeight = 1f;

    [Header("Mood")]
        public float shortTermMemoryDuration = 3f;
        
        public float shortTermMemoryWeight = 1f;
        public float butterflyHealthWeight = 1f;
        public float stanceWeight = 1f;

}
