using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CanvasGroupTrackClip : PlayableAsset, ITimelineClipAsset
{
    public CanvasGroupTrackBehaviour template = new CanvasGroupTrackBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CanvasGroupTrackBehaviour>.Create (graph, template);
        return playable;
    }
}
