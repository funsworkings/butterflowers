using System.Collections.Generic;
using butterflowersOS.Core;
using butterflowersOS.Objects.Managers;
using UnityEngine;
using UnityEngine.Audio;
using uwu.Audio;
using uwu.Snippets.Load;
using uwu.Timeline.Core;

namespace butterflowersOS.Audio
{
	public class GameSceneAudioManager : SceneAudioManager
	{
		// Properties

		[Header("Scene")] 
		Loader Loader;
		Cutscenes Cutscenes;
		
		[Header("Audio")]
		[SerializeField] AudioFader _loadFader = null;
		[SerializeField] AudioFader _gameFader = null;
		[SerializeField] AudioFader _cutsceneFader = null;


		protected override void Start()
		{
			Loader = Loader.Instance;
			Cutscenes = FindObjectOfType<Cutscenes>();
			
			AdjustFadersFromState();
			
			base.Start();
		}
		
		protected override void Update()
		{
			AdjustFadersFromState();
			
			base.Update();
		}
		
		
		#region Mixing

		void AdjustFadersFromState()
		{
			if(Loader.IsLoading)
			{
				_loadFader.FadeIn();
				_gameFader.FadeOut();
				_cutsceneFader.FadeOut();
			}
			else 
			{
				_loadFader.FadeOut();
				
				if (Cutscenes.playing) 
				{
					_cutsceneFader.FadeIn();
					_gameFader.FadeOut();
				}
				else 
				{
					_cutsceneFader.FadeOut();
					_gameFader.FadeIn();
				}
			}
		}
		
		#endregion
	}
}