using butterflowersOS.Objects.Managers;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace butterflowersOS.Visuals.Timeline.Yves
{
	[TrackClipType(typeof(YvesControlAsset))]
	[TrackBindingType(typeof(YvesManager))]
	public class YvesControlTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return ScriptPlayable<YvesControlMixerBehaviour>.Create(graph, inputCount);
		}
	}
}