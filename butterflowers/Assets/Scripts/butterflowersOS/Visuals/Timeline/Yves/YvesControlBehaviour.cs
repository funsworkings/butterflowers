using butterflowersOS.Objects.Managers;
using UnityEngine;
using UnityEngine.Playables;

namespace butterflowersOS.Visuals.Timeline.Yves
{
	[System.Serializable]
	public class YvesControlBehaviour : PlayableBehaviour
	{
		[Range(0f, 1f)] public float depth, blend, brightness = 1f;
		[Min(0f)] public float splits = 0;
	}
}