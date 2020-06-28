using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FocusControlClip : PlayableAsset
{
    public ExposedReference<FocalPoint> focalPoint;
    public bool enabled = false;

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FocusControlBehaviour>.Create (graph);
       
        FocusControlBehaviour clone = playable.GetBehaviour ();
        clone.focalPoint = focalPoint.Resolve (graph.GetResolver ());
        clone.enable = enabled;

        return playable;
    }
}
