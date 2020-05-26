using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioHandler: MonoBehaviour {
    AudioSource audioSource;

    #region Properties

    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] AudioClip[] audioClips;

    #endregion

    #region Attributes

    [SerializeField] protected bool overrideMixer = false;
    [SerializeField] bool debug = false;

    [Header("Pitch Attributes")]
        [SerializeField] protected Vector2 pitchRange = new Vector2(0.9f, 1.1f);
        [SerializeField] float m_pitch = 1f;
        [SerializeField] protected bool smoothPitch = false;
        [SerializeField] protected float pitchSmoothSpeed = 1f;

    [Header("Volume Attributes")]
        [SerializeField] float m_volume = 1f;
        [SerializeField] protected bool smoothVolume = false;
        [SerializeField] protected float volumeSmoothSpeed = 1f;

    [Header("Mixer Attributes")]
        [SerializeField] protected string pitchParam = null;
        [SerializeField] protected string volumeParam = null;

    #endregion

    #region Accessors

    public float currentPitch {
        get
        {
            if (smoothPitch) {
                if (overrideMixer) {
                    var mixer = Mixer;
                    if (mixer != null) {
                        float pitch = 1f;
                        bool success = mixer.GetFloat(pitchParam, out pitch);

                        if (success)
                            return pitch;
                    }
                }

                return audioSource.pitch;
            }

            return m_pitch;
        }
    }

    public float pitch {
        get
        {
            return m_pitch;
        }
        set
        {
            m_pitch = ClampPitchToRange(value);
        }
    }

    public float minPitch {
        get
        {
            return pitchRange.x;
        }
    }

    public float maxPitch {
        get
        {
            return pitchRange.y;
        }
    }

    public float currentVolume {
        get
        {
            if (smoothVolume) {
                if (overrideMixer) {
                    var mixer = Mixer;
                    if (mixer != null) {
                        float volume = 1f;
                        bool success = mixer.GetFloat(volumeParam, out volume);

                        if (success)
                            return volume;
                    }
                }

                return audioSource.volume;
            }

            return m_volume;
        }
    }

    public float volume {
        get
        {
            return m_volume;
        }

        set
        {
            m_volume = Mathf.Clamp01(value);
        }
    }

    public AudioMixer Mixer {
        get
        {
            return (mixerGroup == null) ? null : mixerGroup.audioMixer;
        }
    }

	#endregion

	#region Monobehaviour callbacks

	protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        //set mixergroup 
        if (audioSource.outputAudioMixerGroup == null && mixerGroup != null)
            audioSource.outputAudioMixerGroup = mixerGroup;

        onAwake();
    }

    protected virtual void Update()
    {
        if (smoothPitch) SmoothPitch();
        else SetPitch();

        if (smoothVolume) SmoothVolume();
        else SetVolume();
    }

    #endregion

    #region Internal callbacks

    protected virtual void onAwake() { }

    #endregion

    #region Pitch callbacks

    void SmoothPitch()
    {
        var current = currentPitch;
        var target = Mathf.Lerp(current, pitch, Time.deltaTime * pitchSmoothSpeed);

        SetPitch(target);
    }

    void SetPitch(float pitch = -1f)
    {
        if (pitch < 0f) pitch = this.pitch;

        if (overrideMixer) {
            var mixer = Mixer;
            if (mixer != null) {
                mixer.SetFloat(pitchParam, pitch);
            }                
            return;
        }

        audioSource.pitch = pitch; // Set audio source pitch if mixer not set
    }

    #endregion

    #region Volume callbacks

    void SmoothVolume()
    {
        var current = currentVolume;
        var target = Mathf.Lerp(current, volume, Time.deltaTime * volumeSmoothSpeed);

        SetVolume(target);
    }

    void SetVolume(float volume = -1f)
    {
        if (volume < 0f) volume = this.volume;

        if (overrideMixer) {
            var mixer = Mixer;
            if (mixer != null) {
                float attenuation = (volume >= 1f) ? 0f : -80f; // Remap to attenuation instead of raw volume 
                //mixer.SetFloat(volumeParam, attenuation);
            }
            return;
        }

        audioSource.volume = volume; // Set audio source volume if mixer not set
    }

	#endregion

	#region Sound operations

	public virtual void PlaySound(AudioClip sound, float volume = 1f)
    {
        audioSource.PlayOneShot(sound, volume);
    }

    public void PlaySoundRandomPitch(AudioClip sound, float volume = -1f)
    {
        if (sound == null) return;
        if (volume < 0f) volume = this.volume;

        RandomizePitch(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(sound, volume);
    }

    public void PlayRandomSound(float volume = 1f)
    {
        PlayRandomSound(audioClips, volume);
    }

    public void PlayRandomSound(AudioClip[] sounds, float volume = -1f)
    {
        if (sounds == null || sounds.Length == 0) return;
        if (volume < 0f) volume = this.volume;

        AudioClip sound = sounds[Random.Range(0, sounds.Length)];
        audioSource.PlayOneShot(sound, volume);
    }

    public void PlayRandomSoundRandomPitch(float volume = -1f)
    {
        PlayRandomSoundRandomPitch(audioClips, volume);
    }

    public void PlayRandomSoundRandomPitch(AudioClip[] sounds, float volume = -1f)
    {
        if (sounds == null || sounds.Length == 0) return;
        if (volume < 0f) volume = this.volume;

        m_pitch = RandomPitch();

        AudioClip sound = sounds[Random.Range(0, sounds.Length)];
        audioSource.PlayOneShot(sound, volume);
    }

    #endregion

    #region Pitch operations

    public void RandomizePitch(float min = -1f, float max = -1f)
    {
        // Ensure pitch is containted within range if not set
        if (min == -1f) min = pitchRange.x;
        if (max == -1f) max = pitchRange.y;

        m_pitch = Random.Range(min, max);
    }

    #endregion

    #region Helpers

    float RandomPitch()
    {
        return Random.Range(pitchRange.x, pitchRange.y);
    }

	float ClampPitchToRange(float value)
    {
        return Mathf.Clamp(value, pitchRange.x, pitchRange.y);
    }

	#endregion
}

