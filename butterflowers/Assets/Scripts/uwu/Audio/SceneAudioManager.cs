using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Snippets.Load;

public class SceneAudioManager : MonoBehaviour, ILoadDependent
{
    public static SceneAudioManager AudioManager = null;
    
    #region Internal

    public enum FadeOp
    {
        Nothing,
        
        FadeIn,
        FadeOut
    }
    
    #endregion
    
    // Properties

    [SerializeField] AudioMixer mixer;
    [SerializeField] string mixerVolumeParam;
    
    // Attributes

    [Header("Audio adjustments")] 
    
    [SerializeField] float minVolume = -80f;
    [SerializeField] float maxVolume = 20f;
    [SerializeField] float defaultVolume = 0f;
                     float _volume = 0f;

    [SerializeField] FadeOp _fadeOp = FadeOp.Nothing;
    [SerializeField] bool fadeInOnAwake = true;
    [SerializeField] float fadeInSpeed = -1f, fadeOutSpeed = -1f;

    #region Load dependencies

    float VolumeRange => (maxVolume - minVolume);

    float TargetVolume
    {
        get
        {
            if (_fadeOp == FadeOp.Nothing) return _volume;
            if (_fadeOp == FadeOp.FadeIn) return maxVolume;

            return minVolume;
        }
    }
    
    public float Progress => 1f - (Mathf.Abs(TargetVolume - _volume) / VolumeRange);
    public bool Completed => (_fadeOp == FadeOp.Nothing) || Progress >= 1f;
    
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        AudioManager = this; // Assign audio manager

        _volume = defaultVolume;
        
        ClampMixerVolume();
        AdjustMixerVolume();
        
        if(fadeInOnAwake) FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        if (_fadeOp == FadeOp.Nothing) return;

        float dt = Time.unscaledDeltaTime;

        if (_fadeOp == FadeOp.FadeIn) 
        {
            _volume += dt * fadeInSpeed;
            if (_volume > maxVolume) _fadeOp = FadeOp.Nothing;
        }    
        else 
        {
            _volume -= dt * fadeOutSpeed;
            if (_volume < minVolume) _fadeOp = FadeOp.Nothing;
        }
        
        ClampMixerVolume();
        AdjustMixerVolume();
    }
    
    #region Fade operations

    public void FadeIn()
    {
        if (fadeInSpeed > 0f) 
        {
            _fadeOp = FadeOp.FadeIn;
            return;
        }

        _fadeOp = FadeOp.Nothing;
        _volume = maxVolume;
    }

    public void FadeOut()
    {
        if (fadeOutSpeed > 0f) 
        {
            _fadeOp = FadeOp.FadeOut;
            return;
        }

        _fadeOp = FadeOp.Nothing;
        _volume = minVolume;
    }

    public void FadeCancel()
    {
        _fadeOp = FadeOp.Nothing;
    }
    
    #endregion
    
    #region Volume adjustments

    void ClampMixerVolume()
    {
        _volume = Mathf.Clamp(_volume, minVolume, maxVolume);
    }

    void AdjustMixerVolume()
    {
        mixer.SetFloat(mixerVolumeParam, _volume);
    }
    
    #endregion
}
