using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using Wizard;

[CreateAssetMenu(fileName = "New Brain Preset", menuName = "Settings/Presets/Brain", order = 52)]
public class BrainPreset : ScriptableObject
{
    [Header("Time")]
        public float daysUntilEnvironmentKnowledge = 7f;
        public float daysUntilFileKnowledge = 7f;

    [Header("Multipliers")]
        public float defaultLearningMultiplier = 1f;
        public float nestLearningMultiplier = 1f;
        [Range(0f, 1f)] public float defaultShortTermMemoryEffect = 1f;
        [Range(0f, 1f)] public float nestShortTermMemoryEffect = 1f;

    [Header("Stance")]
        public float environmentWeight = 1f;
        public float filesWeight = 1f;

    [Header("Mood")]
        public float moodSmoothSpeed = 1f;
        public float shortTermMemoryDecaySpeed = 3f;
        [Range(0f, 1f)] public float shortTermMemoryWeightTriggerThreshold = 0f;
        
        public float shortTermMemoryWeight = 1f;
        public float butterflyHealthWeight = 1f;
        public float stanceWeight = 1f;

    [Header("Actions")]
        public float minimumTimeBetweenActions = 1f;
        public float maximumTimeBetweenActions = 3f;

        [Range(0f, 1f)] public float minimumBeaconActivateKnowledge = .5f;
        [Range(0f, 1f)] public float maximumBeaconDeleteKnowledge = .5f;

        [Range(0f, 1f)] public float minimumDayNightActionProbability = .5f, maximumDayNightActionProbability = .5f;
        [Range(0f, 1f)] public float minimumResponseActionProbability = .5f, maximumResponseActionProbability = .5f;

        public AnimationCurve actionProbabilityStanceCurve;

    [Header("Action thresholds")]

        public Brain.ActionStanceThreshold[] actionStanceThresholds;
        public Brain.BeaconOpMoodThreshold[] beaconOpMoodThresholds;
        public Brain.NestOpMoodThreshold[] nestOpMoodThresholds;
        public Brain.EmoteMoodThreshold[] emoteMoodThresholds;
}
