using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Audio;
using uwu.Snippets.Load;

public class SceneAudioManager : AudioFader, ILoadDependent
{
    public static SceneAudioManager AudioManager = null;

    #region Load dependencies

    public float Progress => 1f - (Mathf.Abs(TargetVolume - _volume) / VolumeRange);
    public bool Completed => (_fadeOp == FadeOp.Nothing) || Progress >= 1f;
    
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        AudioManager = this; // Assign audio manager

        base.Start();
    }
}
