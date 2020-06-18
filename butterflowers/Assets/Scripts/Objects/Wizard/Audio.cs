using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

public class Audio: AudioHandler {

    #region External

    [SerializeField] AudioHandler MemoryAudio;
    [SerializeField] Focus Focus;

    #endregion

    #region Properties

    Controller controller;

    Brain brain;
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
        Focus = FindObjectOfType<Focus>();
        brain = controller.Brain;
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

    void SmoothMemoryAudio() { Focus.SetMemoryVolumeWithDistance(brain.shortterm_weight); }

	#endregion
}
