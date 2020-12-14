using System.Collections.Generic;
using Neue.Agent.Brain.Types;
using Neue.Reference.Types;
using Neue.UI;
using UnityEngine;

namespace Neue.Agent.Presets
{
	[CreateAssetMenu(fileName = "New AI Brain", menuName = "Settings/Presets/AI_Brain", order = 0)]
	public class BrainPreset : ScriptableObject
	{
		[Header("Needs")]	
			public float needMultiplier = 10f;
			public float recoveryMultiplier = 1f;
			public float needDecayMultiplier = 1f;
		
			[Tooltip("Describes how different needs are scored for agent")]
			public AnimationCurve needAttenuationCurve;

			[Tooltip("Describes the maximum score for a need for agent, i.e. 100 Health")] 
			public int maximumNeedsScore = 100;

			public List<Frame> excludeDecayNeeds = new List<Frame>();

		[Header("Profiling")] 
			public float baselineDeltaPercentage = 1f;
		
		[Header("Learning")]	
			public float nestLearningMultiplier = 2f;
			
			public int daysUntilEnvironmentKnowledge = 7;
			public int daysUntilFileKnowledge = 7;

		[Header("Stance")]
			public float enviroKnowledgeStanceWeight = 1f;
			public float fileKnowledgeStanceWeight = 1f;
			
			[Range(0f, 1f)] public float lowStanceThreshold = -.5f;
			[Range(0f, 1f)]  public float highStanceThreshold = .5f;
			
		[Header("Mood")]
			public float stanceMoodWeight = 1f;
			public float healthOfButterflowersMoodWeight = 1f;
			public float needsWeight = 1f;
			
			public float moodSmoothSpeed = 1f;
			
			[Range(-1f, 0f)] public float lowMoodThreshold = -.5f;
			[Range(0f, 1f)]  public float highMoodThreshold = .5f;

		[Header("Actions")] 
			public float actionRadius = 1f;
			
			public float minimumTimeBetweenActions = 1f;
			public float maximumTimeBetweenActions = 3f;

			public float baseFatigueTeleportDistance = 10f;

			[Header("Events")] 
			public int eventNullFramesThreshold = 72;
			public int eventMaximumStackHeight = 12;

		[Header("UI")] 
			public MoodIconMapping[] moodAttributeIcons;



			#region Accessors

			public void AssignMoodAttributeIcon(Mood mood, out Sprite image, out Color color)
			{
				foreach (MoodIconMapping map in moodAttributeIcons) {
					if (map.mood == mood) 
					{
						image = map.image;
						color = map.color;
						return;
					}
				}

				image = null;
				color = Color.white;
			}

			#endregion

	}
}