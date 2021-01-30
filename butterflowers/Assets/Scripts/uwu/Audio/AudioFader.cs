using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace uwu.Audio
{
	public class AudioFader : MonoBehaviour
	{
		#region Internal

		public enum FadeOp
		{
			Nothing,
        
			FadeIn,
			FadeOut
		}
    
		#endregion
		
		// Events

		public UnityEvent DidFadeIn, DidFadeOut;
		
		// Properties

		[SerializeField] protected AudioMixer mixer; 
		[SerializeField] protected AudioSource audio;
		[SerializeField] string mixerVolumeParam;

		[SerializeField] bool useMixerGroup = true;
		[SerializeField] bool useScaledTime = false;

		// Attributes
		
		[Header("Audio adjustments")] 
    
		[SerializeField] float minVolume = -80f;
		[SerializeField] float maxVolume = 20f;
		[SerializeField] float defaultVolume = 0f;
		protected float _volume = 0f;

		[SerializeField] protected FadeOp _fadeOp = FadeOp.Nothing;
		[SerializeField] bool fadeInOnAwake = true;
		[SerializeField] float fadeInSpeed = -1f, fadeOutSpeed = -1f;
		
		#region Accessors
		
		protected float TargetVolume
		{
			get
			{
				if (_fadeOp == FadeOp.Nothing) return _volume;
				if (_fadeOp == FadeOp.FadeIn) return maxVolume;

				return minVolume;
			}
		}
		
		protected float VolumeRange => (maxVolume - minVolume);
		
		#endregion

		protected virtual void Awake()
		{
			if(audio == null) audio = GetComponent<AudioSource>();
		}

		protected virtual void Start()
		{
			_volume = defaultVolume;
        
			ClampVolume();
			AdjustVolume();
        
			if(fadeInOnAwake) FadeIn();
		}
		
		// Update is called once per frame
		protected virtual void Update()
		{
			if (_fadeOp == FadeOp.Nothing) return;

			float dt = (useScaledTime)? Time.deltaTime : Time.unscaledDeltaTime;

			if (_fadeOp == FadeOp.FadeIn) 
			{
				_volume += dt * fadeInSpeed;
				if (_volume > maxVolume) 
				{
					_fadeOp = FadeOp.Nothing;
					DidFadeIn.Invoke();
				}
			}    
			else 
			{
				_volume -= dt * fadeOutSpeed;
				if (_volume < minVolume) 
				{
					_fadeOp = FadeOp.Nothing;
					DidFadeOut.Invoke();
				}
			}
        
			ClampVolume();
			AdjustVolume();
		}
		
		#region Fade operations

		public void FadeIn()
		{
			if (_fadeOp == FadeOp.FadeIn) return; // Ignore repeat fade-in requests
			
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
			if (_fadeOp == FadeOp.FadeOut) return; // Ignore repeat fade-in requests
			
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

		void ClampVolume()
		{
			_volume = Mathf.Clamp(_volume, minVolume, maxVolume);
		}

		void AdjustVolume()
		{
			if (useMixerGroup)
				mixer.SetFloat(mixerVolumeParam, _volume);
			else
				audio.volume = _volume;
		}
    
		#endregion
	}
}