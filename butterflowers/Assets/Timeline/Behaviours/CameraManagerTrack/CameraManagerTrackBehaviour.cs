using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using uwu.Camera;

[Serializable]
public class CameraManagerTrackBehaviour : PlayableBehaviour
{
    public GameCamera camera;

    bool sent = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        sent = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        CameraManager Cams = (CameraManager)playerData;

        if (Cams != null) {

            if (!sent) {
                Cams.SetCamera(camera);
                sent = true;
            }
        }
    }
}
