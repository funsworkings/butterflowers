using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Wizard;

[CreateAssetMenu(fileName = "New Wizard Preset", menuName = "Settings/Presets/Wizard", order = 52)]
public class WizardPreset : ScriptableObject
{
	#region Internal

	[System.Serializable]
    public class MemoryLookup 
    {
        public string id = null;
        public Memory memory;
    }

    #endregion

    [Header("Memory")]
        public Memory[] memories = new Memory[] { };
}
