using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class PushDialogueBehaviour : PlayableBehaviour
{
    public string[] dialogueParams;
    public bool autoprogress = false;

    bool sent = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        sent = false;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;

        DialogueHandler Dialogue = (DialogueHandler)playerData;

        if (Dialogue != null) {

            Dialogue.autoprogress = autoprogress;

            if (!sent) {
                for (int i = 0; i < dialogueParams.Length; i++)
                    Dialogue.Push(dialogueParams[i]);

                sent = true;
            }
        }
    }
}
