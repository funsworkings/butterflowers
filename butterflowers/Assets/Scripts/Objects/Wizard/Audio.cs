using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

public class Audio : AudioHandler
{
    #region Properties

    Dialogue Dialogue;

	#endregion

    protected override void Awake()
    {
        base.Awake();

        Dialogue = GetComponent<Dialogue>();
    }

    void OnEnable()
    {
        if(Dialogue != null) Dialogue.onProgress += delegate(string body) { PlayRandomSoundRandomPitch(); };
    }

    void OnDisable()
    {
        if (Dialogue != null) Dialogue.onProgress -= delegate (string body) { PlayRandomSoundRandomPitch(); };
    }
}
