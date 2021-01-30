using System.Collections.Generic;
using butterflowersOS.Core;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Timeline.Core;

namespace butterflowersOS.Audio
{
	public class GameSceneAudioManager : SceneAudioManager
	{
		
		#region Internal

		[System.Serializable]
		public struct StateMixerVolume
		{
			public World.State state;

			public float cutsceneVolume;
			public float gameVolume;
			public float loadVolume;
			public float eodVolume;
		}

		[System.Serializable]
		public struct StateMixer
		{
			public AudioMixer mixer;
			public string volumeParam;
			public float volume;
			public float volumeSmoothSpeed;

			public void SmoothVolume(float dt, float t_vol)
			{
				volume = Mathf.Lerp(volume, t_vol, dt * volumeSmoothSpeed);
				SetVolume(volume);
			}

			public void SetVolume(float vol)
			{
				mixer.SetFloat(volumeParam, vol);
			}
		}
		
		#endregion
		
		// External

		World World;

		[Header("Parameters")] 
			[SerializeField] List<StateMixerVolume> settings = new List<StateMixerVolume>();
			[SerializeField] StateMixer loadMixer;
			[SerializeField] StateMixer cutsceneMixer;
			[SerializeField] StateMixer gameMixer;
			[SerializeField] StateMixer eodMixer;

		void Start()
		{
			World = World.Instance;
			EvaluateMixers(World._State);
		}
			
		void Update()
		{
			var state = World._State;
			EvaluateMixers(state);
		}
		
		
		#region Mixing

		void EvaluateMixers(World.State state)
		{
			float dt = Time.deltaTime;
		
			foreach (StateMixerVolume setting in settings) 
			{
				if (setting.state != state) continue;
				
				loadMixer.SmoothVolume(dt, setting.loadVolume);
				cutsceneMixer.SmoothVolume(dt, setting.cutsceneVolume);
				gameMixer.SmoothVolume(dt, setting.gameVolume);
				eodMixer.SmoothVolume(dt, setting.eodVolume);
			}
		}
		
		#endregion
	}
}