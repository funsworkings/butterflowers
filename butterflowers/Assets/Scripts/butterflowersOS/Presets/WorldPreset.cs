using butterflowersOS.Data;
using Neue.Reference.Types;
using UnityEngine;

namespace butterflowersOS.Presets
{
    [CreateAssetMenu(fileName = "New World Preset", menuName = "Settings/Presets/World", order = 52)]
    public class WorldPreset : ScriptableObject
    {
        [Header("Debug")]
            public bool persistDiscoveries = false;
            public bool resetWorldClock = false;
            public bool persistDialogue = false;
            public bool persistDialogueVisited = false;
            public bool persistKnowledge = false;
            public bool persistBeacons = false;
            public bool persistVines = false;
            public bool persistSequence = false;
            public bool overrideSequence = false;
            public Frame overrideSequenceFrame;
            public bool alwaysIntro = false;
            public bool alwaysAbsorb = false;
            public bool useWizard = false;
            public bool takePhotos = false;
            public bool logEvents = false;
            public bool loadTexturesInEditor = true;
            public bool loadThumbnailsInEditor = true;
            public bool generateThumbnailsInEditor = true;
            public bool useDesktopFilesForDebugBeacons = true;
            public bool allowDebugSpawn = true;
            public bool allowDebugTimeSkip = true;
            public bool allowExternalNeueagent = true;

        [Header("Time Attributes")]
            public float hoursPerDay = 24f;
            [Range(0f, 1f)] public float startOfDay = .25f, endOfDay = .75f;
            public float sunHoursPerDay = 1f;

        [Header("Physics Attributes")]
            public Vector3 directionOfGravity = Vector3.down;

        [Header("Texture Attributes")] 
            public Texture2D defaultNullTexture;
            public Texture2D[] defaultTextures;

        [Header("Spawn Attributes")]
            public int amountOfButterflies = 100;

        [Header("Nest Attributes")]
            public int nestCapacity = 6;

        [Header("Beacon Attributes")]
            public int amountOfBeacons = 10;
            public float normalBeaconScale = 1f;
            public float beaconLerpDuration = 1f;
            public AnimationCurve beaconScaleCurve;

        [Header("Vine Attributes")] 
            [Range(0f, 1f)] public float saplingGrowHeight;
            public float daysToGrowVine = 1f;
            public float minimumVineGrowHeight = 1f;
            public float maximumVineGrowHeight = 10f;
            public float vineWidth = 1f;
            public int minimumLeavesPerSegment = 5, maximumLeavesPerSegment = 10;
            [Range(0f, 1f)] public float leafDensityPerSegment;

        [Header("Leaf Attributes")] 
            public AnimationCurve leafGrowCurve;
            public float leafGrowTime = 1f;
            public float minimumLeafScale = .33f, maximumLeafScale = 1f;
            public float leafFlutterAngle = 45f;
            public float leafFlutterTime = 1f;
            
        [Header("Surveillance Attributes")] 
            public float surveillanceLogRate = 1f;
            public bool backlogTextures = true;

            public AnimationCurve discoveryScoreCurve;
            public AnimationCurve beaconsAddScoreCurve;
            public AnimationCurve beaconsPlantScoreCurve;
            public AnimationCurve beaconsDestroyScoreCurve;
            public AnimationCurve beaconsFlowerScoreCurve;
            public AnimationCurve nestKickScoreCurve;
            public AnimationCurve nestSpillScoreCurve;

            public float eventStackHeight = 64f;
            public AnimationCurve eventStackScoreCurve;

            public float currentLogWeight = 0f;
            public float averageLogWeight = 1f;
            
        [Header("Scoring Attributes")]
            public CompositeSurveillanceData baselineSurveillanceData;

        [Header("Unknown Attributes")]
            [Range(0f, 1f)] public float unknownPersistenceThreshold = .5f;

        [Header("Remote Attributes")] 
            public float cursorSpeedThreshold = 1f;
            public float cursorIdleTimeThreshold = 10f;
            public float cursorIdleDelay = 1f;

        [Header("Miscellaneous")] 
            public Texture2D[] worldTextures;

        #region Time conversions

        public float minutesPerDay {
            get
            {
                return hoursPerDay * 60f;
            }
        }

        public float secondsPerDay
        {
            get { return minutesPerDay * 60f; }
        }

        public float ConvertSecondsToDays(float seconds)
        {
            return (seconds / secondsPerDay);
        }

        public float ConvertDaysToSeconds(float days)
        {
            return (days * secondsPerDay);
        }

        public bool IsDuringDay(float timeofday)
        {
            return (timeofday > startOfDay && timeofday <= endOfDay);
        }

        public bool IsDuringNight(float timeofday)
        {
            return (timeofday <= startOfDay || timeofday > endOfDay);
        }

        #endregion
    }

}
