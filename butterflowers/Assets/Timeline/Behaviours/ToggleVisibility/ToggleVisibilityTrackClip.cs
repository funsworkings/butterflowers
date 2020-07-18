using System;
using UIExt.Behaviors;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ToggleVisibilityTrackClip : PlayableAsset
{
    public ExposedReference<ToggleVisibility> vis;
    public bool show = false;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ToggleVisibilityTrackBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        ToggleVisibility visibility = vis.Resolve(graph.GetResolver());
        behaviour.visibility = visibility;
        behaviour.show = show;

        return playable;
    }
}
