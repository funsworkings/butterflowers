﻿using UnityEngine;
using System.Collections;

namespace Settings
{
    [CreateAssetMenu(fileName = "New World Preset", menuName = "Settings/World Preset", order = 52)]
    public class WorldPreset : ScriptableObject
    {
        public float hoursPerDay = 24f;

        public float minutesPerDay
        {
            get
            {
                return hoursPerDay * 60f;
            }
        }

        public float secondsPerDay
        {
            get
            {
                return minutesPerDay * 60f;
            }
        }

        public float ConvertToDays(float seconds)
        {
            return (seconds / secondsPerDay);
        }
    }

}
