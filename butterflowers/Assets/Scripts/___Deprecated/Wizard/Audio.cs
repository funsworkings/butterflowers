using System;
using System.Collections;
using System.Collections.Generic;
using Neue.Agent;
using UnityEngine;
using uwu.Audio;
using Wizard;

[Obsolete("Obsolete API!", true)]
public class Audio: AudioHandler {

    #region External

    [SerializeField] AudioHandler MemoryAudio;
    [SerializeField] Focusing Focusing;

    #endregion

    #region Properties

    Controller controller;

    BrainOld brain;
	Dialogue dialogue;

    #endregion

	#region Monobehaviour callbacks

	protected override void Awake()
    {
        base.Awake();

        controller = GetComponent<Controller>();
        dialogue = GetComponent<Dialogue>();
    }

    void Start() {
        Focusing = FindObjectOfType<Focusing>();
        //brain = controller.Brain;
    }

    void OnEnable()
    {
        if (dialogue != null) dialogue.onProgress += delegate (string body) { PlayRandomSoundRandomPitch(); };
    }

    void OnDisable()
    {
        if (dialogue != null) dialogue.onProgress -= delegate (string body) { PlayRandomSoundRandomPitch(); };
    }

    protected override void Update()
    {
        base.Update();

        SmoothMemoryAudio();
    }

    #endregion

    #region Operations

    public void PushMemoryAudio(AudioClip clip)
    {
        if (clip == null) return;
        MemoryAudio.PlaySound(clip);
    }

    #endregion

    #region Memories

    void SmoothMemoryAudio() { //Focusing.SetMemoryVolumeWithDistance(brain.shortterm_weight);
                               }

	#endregion
}
