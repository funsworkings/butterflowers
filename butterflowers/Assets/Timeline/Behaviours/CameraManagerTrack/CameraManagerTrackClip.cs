using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using uwu.Camera;

[Serializable]
public class CameraManagerTrackClip : PlayableAsset
{
    public ExposedReference<GameCamera> focalPoint;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CameraManagerTrackBehaviour>.Create(graph);

        CameraManagerTrackBehaviour clone = playable.GetBehaviour();
        clone.camera = focalPoint.Resolve(graph.GetResolver());

        return playable;
    }
}
