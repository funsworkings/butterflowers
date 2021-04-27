using butterflowersOS.Objects.Managers;
using UnityEngine;
using UnityEngine.Playables;

namespace butterflowersOS.Visuals.Timeline.Yves
{
	public class YvesControlMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			YvesManager trackBinding = playerData as YvesManager;

			float _depth = 0f, _blend = 0f, _brightness = 0f;
			float _splits = 0f;

			if (!trackBinding)
				return;
 
			int inputCount = playable.GetInputCount (); //get the number of all clips on this track
 
			for (int i = 0; i < inputCount; i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				ScriptPlayable<YvesControlBehaviour> inputPlayable = (ScriptPlayable<YvesControlBehaviour>)playable.GetInput(i);
				YvesControlBehaviour input = inputPlayable.GetBehaviour();
            
				// Use the above variables to process each frame of this playable.
				_depth += input.depth * inputWeight;
				_blend += input.blend * inputWeight;
				_brightness += input.brightness * inputWeight;
				_splits += input.splits * inputWeight;
			}
 
			//assign the result to the bound object
			trackBinding.depth = _depth;
			trackBinding.value = _blend;
			trackBinding.brightness = _brightness;
			trackBinding.splits = Mathf.RoundToInt(_splits);
		}
	}
}