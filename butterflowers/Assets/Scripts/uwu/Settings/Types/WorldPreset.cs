using UnityEngine;
using System.Collections;

namespace Settings
{
    [CreateAssetMenu(fileName = "New World Preset", menuName = "Settings/Presets/World", order = 52)]
    public class WorldPreset : ScriptableObject
    {
        [Header("Debug")]
            public bool memory = false;
            public bool reset = false;

        [Header("Time Attributes")]
            public float hoursPerDay = 24f;
            [Range(0f, 1f)] public float startOfDay = .25f, endOfDay = .75f;
            public float sunHoursPerDay = 1f;

        [Header("Physics Attributes")]
            public Vector3 directionOfGravity = Vector3.down;

        [Header("Spawn Attributes")]
            public int amountOfButterflies = 100;



        #region Time conversions

        public float minutesPerDay {
            get
            {
                return hoursPerDay * 60f;
            }
        }

        public float secondsPerDay {
            get
            {
                return minutesPerDay * 60f;
            }
        }

        public float ConvertToDays(float seconds)
        {
            return (seconds / secondsPerDay);
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
