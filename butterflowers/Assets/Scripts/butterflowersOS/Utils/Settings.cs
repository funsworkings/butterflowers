using System;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Extensions;

namespace butterflowersOS.Utils
{
	public class Settings : uwu.Utilities.Settings
	{
		public static Settings Instance = null;
		
		#region Internal

		[System.Serializable]
		public struct AudioMixerVolumeSetting
		{
			public AudioMixerGroup mixer;
			public string volumeParam;
		}
		
		#endregion
		
		// Properties

		[SerializeField] AudioMixerVolumeSetting[] bgmMixers;
		[SerializeField] AudioMixerVolumeSetting[] sfxMixers;

		float bgm_vol = 0f, sfx_vol = 0f;
		[SerializeField] float minVolume = -80f, maxVolume = 20f;

		protected override object[] Items { get; } = new object[] 
		{
			"bgm_volume", Type.Float, .3f,
			"sfx_volume", Type.Float, .3f
		};

		protected override void Awake()
		{
			if (Instance == null) 
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else 
			{
				Destroy(gameObject);
				return;
			}
			
			base.Awake();
		}

		void LateUpdate()
		{
			float bgv = bgm_vol.RemapNRB(0f, 1f, minVolume, maxVolume);
			float sfxv = sfx_vol.RemapNRB(0f, 1f, minVolume, maxVolume);
			
			foreach (AudioMixerVolumeSetting m in bgmMixers) 
			{
				m.mixer.audioMixer.SetFloat(m.volumeParam, bgv);
			}
			foreach (AudioMixerVolumeSetting m in sfxMixers) 
			{
				m.mixer.audioMixer.SetFloat(m.volumeParam, sfxv);
			}
		}

		void RemapVolume(ref float vol, float weight)
		{
			vol = vol.RemapNRB(minVolume, maxVolume, 0f, 1f);
			vol *= weight;
			vol = vol.RemapNRB(0f, 1f, minVolume, maxVolume);
		}

		protected override void DidApplySetting(string key, Type type, object value)
		{
			Debug.LogWarning("Did apply " + key + "  => " + value);

			if (key == "bgm_volume") 
			{
				bgm_vol = (float) value;
			}
			else if (key == "sfx_mixers") 
			{
				sfx_vol = (float) value;
			}
		}
	}
}