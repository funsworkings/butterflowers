using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ScalerClip : PlayableAsset, ITimelineClipAsset
{
    public ScalerBehaviour template = new ScalerBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ScalerBehaviour>.Create (graph, template);
        ScalerBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
}
