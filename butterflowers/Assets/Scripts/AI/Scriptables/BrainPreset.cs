using UnityEngine;

namespace AI.Scriptables
{
	[CreateAssetMenu(fileName = "New AI Brain", menuName = "Settings/Presets/AI_Brain", order = 0)]
	public class BrainPreset : ScriptableObject
	{
		[Header("Needs")]
		
			[Tooltip("Describes how different needs are scored for agent")]
			public AnimationCurve needAttenuationCurve;

			[Tooltip("Describes the maximum score for a need for agent, i.e. 100 Health")] 
			public int maximumNeedsScore = 100;


		[Header("Learning")] 
		
			public float nestLearningMultiplier = 2f;
	}
}