using System;
using System.Collections.Generic;
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

		[SerializeField] List<AudioMixerVolumeSetting> bgmMixers = new List<AudioMixerVolumeSetting>();
		[SerializeField] List<AudioMixerVolumeSetting> sfxMixers = new List<AudioMixerVolumeSetting>();

		[SerializeField] AudioMixerGroup[] sceneMixers;

		float bgm_vol = 0f, sfx_vol = 0f;
		[SerializeField] float minVolume = -80f, maxVolume = 20f;
		[SerializeField] AnimationCurve volumeCurve;

		protected override object[] Items { get; } = new object[] 
		{
			"bgm_volume", Type.Float, .87f,
			"sfx_volume", Type.Float, .87f
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
			
			BindMixers();
			
			base.Awake();
		}

		void BindMixers()
		{
			foreach (AudioMixerGroup mixer in sceneMixers) 
			{
				var assoc = mixer.audioMixer.FindMatchingGroups(string.Empty);
				foreach (AudioMixerGroup a in assoc) 
				{
					Debug.LogWarningFormat("Found matching mixer for initial: {0} = {1}", mixer.name, a.name);

					string id = a.name;
					string @param = "";
					
					if (id.Contains("BGM_Ctrl")) 
					{
						@param = id += "_Volume";
						bgmMixers.Add(new AudioMixerVolumeSetting(){ mixer = a, volumeParam = @param});
					}
					else if (id.Contains("SFX_Ctrl")) 
					{
						@param = id += "_Volume";
						sfxMixers.Add(new AudioMixerVolumeSetting(){ mixer = a, volumeParam = @param});
					}
				}
			}
		}

		protected override void DidApplySetting(string key, Type type, object value)
		{
			Debug.LogWarning("Did apply " + key + "  => " + value);

			if (key == "bgm_volume") 
			{
				bgm_vol = volumeCurve.Evaluate((float) value);
				
				float bgv = bgm_vol.RemapNRB(0f, 1f, minVolume, maxVolume);
				foreach (AudioMixerVolumeSetting m in bgmMixers) 
				{
					m.mixer.audioMixer.SetFloat(m.volumeParam, bgv);
				}
			}
			else if (key == "sfx_mixers") 
			{
				sfx_vol = volumeCurve.Evaluate((float) value);
				
				float sfxv = sfx_vol.RemapNRB(0f, 1f, minVolume, maxVolume);
				foreach (AudioMixerVolumeSetting m in sfxMixers) 
				{
					m.mixer.audioMixer.SetFloat(m.volumeParam, sfxv);
				}
			}
		}
	}
}