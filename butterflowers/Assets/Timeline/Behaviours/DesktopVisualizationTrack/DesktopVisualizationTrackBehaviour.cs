using Intro;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityStandardAssets.Cameras;

[Serializable]
public class DesktopVisualizationTrackBehaviour : PlayableBehaviour
{
    public enum Op { Null, Spawn, Absorb, Kill }
    public Op op = Op.Null;

    bool sent = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        sent = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        DesktopVisualization Vis = (DesktopVisualization)playerData;

        if (Vis != null) {

            if (!sent) {

                if (op == Op.Spawn)
                    Vis.SpawnDesktopFiles();
                else if (op == Op.Absorb)
                    Vis.AbsorbDesktopFiles();
                else if (op == Op.Kill)
                    Vis.KillAllButterflies();

                sent = true;
            }
        }
    }
}
