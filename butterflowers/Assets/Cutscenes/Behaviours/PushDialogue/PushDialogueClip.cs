using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class PushDialogueClip : PlayableAsset
{
    public PushDialogueBehaviour template = new PushDialogueBehaviour ();

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PushDialogueBehaviour>.Create (graph, template);
        PushDialogueBehaviour clone = playable.GetBehaviour ();

        return playable;
    }
}
