using UnityEngine;
using UnityEngine.Playables;

namespace butterflowersOS.Visuals.Timeline.Yves
{
	public class YvesControlAsset : PlayableAsset
	{
		public YvesControlBehaviour template;
 
		public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
		{
			var playable = ScriptPlayable<YvesControlBehaviour>.Create(graph, template);
			return playable;   
		}
	}
}