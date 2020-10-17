using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using uwu.UI.Behaviors;

[Serializable]
public class ToggleVisibilityTrackBehaviour : PlayableBehaviour
{
    public ToggleVisibility visibility;
    public bool show = false;

    bool sent = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        sent = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        if (visibility != null) {
            if (!sent) {
                if (show)
                    visibility.Show();
                else
                    visibility.Hide();

                sent = true;
            }
        }
    }

}
