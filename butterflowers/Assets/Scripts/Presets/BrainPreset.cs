using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using Wizard;

[CreateAssetMenu(fileName = "New Brain Preset", menuName = "Settings/Presets/Brain", order = 52)]
public class BrainPreset : ScriptableObject
{
	#region Resource allocation

	[Header("STANCE (WEIGHTS)")]
        public float fileKnowledgeStanceWeight = 1f;
        public float playerKnowledgeStanceWeight = 1f;
        public float enviroKnowledgeStanceWeight = 1f;



    [Header("MOOD (WEIGHTS)")]
        public float stanceMoodWeight = 1f;
        public float shortTermMemoryMoodWeight = 1f;
        public float healthOfButterflowersMoodWeight = 1f;




    [Header("LEARNING (TIME)")]
        public float daysUntilEnvironmentKnowledge = 7f;
        public float daysUntilFileKnowledge = 7f;

    [Header("LEARNING (MULTIPLIERS)")]
        public float defaultLearningMultiplier = 1f;
        public float nestLearningMultiplier = 1f;
    
    


    [Header("SHORT TERM MEMORY EFFECTS")]
        [Range(0f, 1f)] public float defaultShortTermMemoryEffect = 1f;
        [Range(0f, 1f)] public float nestShortTermMemoryEffect = 1f;
        [Range(0f, 1f)] public float shortTermMemoryWeightTriggerThreshold = 0f;


    #endregion

    #region Mechanics

    [Header("STANCE (MECHANICS)")]
        [Range(0f, 1f)] public float lowStanceThreshold = -.5f;
        [Range(0f, 1f)]  public float highStanceThreshold = .5f;

    [Header("MOOD (MECHANICS)")]
        [Range(-1f, 0f)] public float lowMoodThreshold = -.5f;
        [Range(0f, 1f)]  public float highMoodThreshold = .5f;

        [Range(-1f, 0f)] public float needsComfortThreshold = -.5f;

        public float moodSmoothSpeed = 1f;
        public float shortTermMemoryDecaySpeed = 3f;

    [Header("ACTIONS (MECHANICS)")]
        public float minimumTimeBetweenActions = 1f;
        public float maximumTimeBetweenActions = 3f;

        [Range(0f, 1f)] public float minimumBeaconActivateKnowledge = .5f;
        [Range(0f, 1f)] public float maximumBeaconDeleteKnowledge = .5f;

        [Range(0f, 1f)] public float minimumDayNightActionProbability = .5f, maximumDayNightActionProbability = .5f;
        [Range(0f, 1f)] public float minimumResponseActionProbability = .5f, maximumResponseActionProbability = .5f;

    [Header("MISCELLANEOUS (MECHANICS)")]
        [Range(0f, 1f)] public float reactionProbability = .5f;

        public float impactActionWeight = 1f;
        public float impactMoodWeight = 1f;
        public float impactHoBWeight = 1f;

	#endregion

	#region  Modules

    
    [Header("LEARNING (MODULE)")]

	#endregion

	#region Miscellaneouss

	[Header("MISCELLANEOUS")]
        [Range(0f, 1f)] public float absorptionThreshold = .95f;
        [Range(0f, 1f)] public float healthOfButterflowersGestureThreshold = .5f;

	#endregion

	[Header("DEPRECATED")]

        public Brain.ActionStanceThreshold[] actionStanceThresholds;
        public Brain.BeaconOpMoodThreshold[] beaconOpMoodThresholds;
        public Brain.NestOpMoodThreshold[] nestOpMoodThresholds;
        public Brain.EmoteMoodThreshold[] emoteMoodThresholds;
}
