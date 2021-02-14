using System;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Extensions;
using Random = UnityEngine.Random;

namespace uwu.Audio
{
	public class AudioHandler : MonoBehaviour
	{
		public float cv;
		AudioSource audioSource;

		#region Internal callbacks

		protected virtual void onAwake()
		{
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

		#region Properties

		[SerializeField] AudioMixerGroup mixerGroup;
		[SerializeField] AudioClip[] audioClips;

		#endregion

		#region Attributes

		[SerializeField] protected bool overrideMixer;
		[SerializeField] bool debug;

		[Header("Pitch Attributes")] [SerializeField]
		protected Vector2 pitchRange = new Vector2(0.9f, 1.1f);

		[SerializeField] float m_pitch = 1f;
		[SerializeField] protected bool smoothPitch;
		[SerializeField] protected float pitchSmoothSpeed = 1f;

		[Header("Volume Attributes")] [SerializeField]
		float m_volume = 1f;

		[SerializeField] protected bool smoothVolume;
		[SerializeField] protected float volumeSmoothSpeed = 1f;

		[Header("Mixer Attributes")] [SerializeField]
		protected string pitchParam;

		[SerializeField] protected string volumeParam;

		#endregion

		#region Accessors

		public float currentPitch
		{
			get
			{
				if (smoothPitch) {
					if (overrideMixer) {
						var mixer = Mixer;
						if (mixer != null) {
							var pitch = 1f;
							var success = mixer.GetFloat(pitchParam, out pitch);

							if (success)
								return pitch;
						}
					}

					return audioSource.pitch;
				}

				return m_pitch;
			}
		}

		public float pitch
		{
			get => m_pitch;
			set => m_pitch = ClampPitchToRange(value);
		}

		public float minPitch => pitchRange.x;

		public float maxPitch => pitchRange.y;

		public float currentVolume
		{
			get
			{
				if (smoothVolume) {
					if (overrideMixer) {
						var mixer = Mixer;
						if (mixer != null) {
							var vol = 1f;
							var success = mixer.GetFloat(volumeParam, out vol);

							if (success)
								return vol;
						}
					}

					return audioSource.volume;
				}

				return m_volume;
			}
		}

		public float volume
		{
			get => m_volume;

			set => m_volume = Mathf.Clamp01(value);
		}

		public AudioMixer Mixer => mixerGroup == null ? null : mixerGroup.audioMixer;

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
				if (mixer != null)
					try {
						mixer.SetFloat(pitchParam, pitch);
					}
					catch (Exception e) {
						Debug.LogWarning(e.Message);
					}

				return;
			}

			audioSource.pitch = pitch; // Set audio source pitch if mixer not set
		}

		#endregion

		#region Volume callbacks

		void SmoothVolume()
		{
			var current = FromDecibels(currentVolume);
			var target = Mathf.Lerp(current, volume, Time.deltaTime * volumeSmoothSpeed);

			SetVolume(target);
		}

		void SetVolume(float volume = -1f)
		{
			if (volume < 0f) volume = this.volume;

			if (overrideMixer) {
				var mixer = Mixer;
				if (mixer != null) {
					var attenuation = ToDecibels(volume); // Remap to attenuation instead of raw volume    
					mixer.SetFloat(volumeParam, attenuation);
				}

				return;
			}

			audioSource.volume = volume; // Set audio source volume if mixer not set
		}

		#endregion

		#region Sound operations

		public virtual void PlaySound(AudioClip sound)
		{
			audioSource.PlayOneShot(sound, volume);
		}

		public void PlaySoundRandomPitch(AudioClip sound)
		{
			if (sound == null) return;

			RandomizePitch(pitchRange.x, pitchRange.y);
			audioSource.PlayOneShot(sound, volume);
		}

		public void PlayRandomSound()
		{
			PlayRandomSound(audioClips, volume);
		}

		public void PlayRandomSound(AudioClip[] sounds, float volume = -1f)
		{
			if (sounds == null || sounds.Length == 0) return;
			if (volume < 0f) volume = this.volume;

			var sound = sounds[Random.Range(0, sounds.Length)];
			audioSource.PlayOneShot(sound, volume);
		}

		public void PlayRandomSoundRandomPitch()
		{
			PlayRandomSoundRandomPitch(audioClips, volume);
		}

		public void PlayRandomSoundRandomPitch(AudioClip[] sounds, float volume = -1f)
		{
			if (sounds == null || sounds.Length == 0) return;
			if (volume < 0f) volume = this.volume;

			m_pitch = RandomPitch();

			var sound = sounds[Random.Range(0, sounds.Length)];
			audioSource.PlayOneShot(sound, volume);
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

		float ToDecibels(float volume)
		{
			return volume.RemapNRB(0f, 1f, -80f, 0f);
		}

		float FromDecibels(float decibels)
		{
			return decibels.RemapNRB(-80f, 0f, 0f, 1f);
		}

		#endregion
	}
}