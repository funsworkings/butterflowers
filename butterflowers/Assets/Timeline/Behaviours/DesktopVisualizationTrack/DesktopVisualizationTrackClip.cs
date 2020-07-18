using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class DesktopVisualizationTrackClip : PlayableAsset
{
    public DesktopVisualizationTrackBehaviour template = new DesktopVisualizationTrackBehaviour ();

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DesktopVisualizationTrackBehaviour>.Create (graph, template);

        return playable;
    }
}
