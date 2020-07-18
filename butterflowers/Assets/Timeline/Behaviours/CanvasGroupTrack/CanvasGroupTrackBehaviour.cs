using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CanvasGroupTrackBehaviour : PlayableBehaviour
{
    [Range(0f, 1f)]
    public float alpha;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        CanvasGroup canvasGroup = (playerData as CanvasGroup);

        if (canvasGroup != null) {
            canvasGroup.alpha = alpha;
        }
    }
}
