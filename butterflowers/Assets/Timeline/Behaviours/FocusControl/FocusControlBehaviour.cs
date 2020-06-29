using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FocusControlBehaviour : PlayableBehaviour
{
    public FocalPoint focalPoint;
    public bool enable;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        if (enable) 
        {
            if (focalPoint != null) {
                focalPoint.Focus();
            }
        }
        else {
            Focus focus = (Focus)playerData;

            if(focus != null)
                focus.LoseFocus();
        }
    }

}
