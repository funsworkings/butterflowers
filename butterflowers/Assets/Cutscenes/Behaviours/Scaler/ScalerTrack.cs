using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.3290762f, 0.5749155f, 0.8207547f)]
[TrackClipType(typeof(ScalerClip))]
[TrackBindingType(typeof(Transform))]
public class ScalerTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<ScalerMixerBehaviour>.Create (graph, inputCount);
    }
}
