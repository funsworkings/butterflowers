using System;
using System.Collections;
using System.Linq;
using butterflowersOS.Core;
using butterflowersOS.Interfaces;
using butterflowersOS.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu.Snippets.Load;
using uwu.Timeline.Core;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class PauseMenu : GenericMenu
	{
		// External

		[SerializeField] Cutscenes cutscenes = null;
		[SerializeField] SceneAudioManager sceneAudio = null;
		
		// Properties

		ToggleOpacity opacity;
		[SerializeField] GameObject teleporter = null;
		[SerializeField] TMP_Text exitTextElement = null;
		[SerializeField] Slider _bgmVolume = null, _sfxVolume = null;

		bool disposeInProgress = false;
		bool didPauseCutscenes = false;

		public bool IsActive => IsVisible || Dispose;
		public bool Dispose => disposeInProgress;
		
		// Attributes

		[SerializeField] int teleportSceneIndex = 0;
		
		
		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		protected override void Start()
		{
			disposeInProgress = false;
			
			Close();
		}

		void Update()
		{
			if (Input.GetKeyUp(KeyCode.Escape) &&  !disposeInProgress) 
				Toggle();
		}

		void OnDestroy()
		{
			AudioListener.pause = false; // Discard audio listener changess
		}

		#region Menu

		bool didLoad = false;

		protected override void DidOpen()
		{
			didLoad = false;
			opacity.Show();
			
			AudioListener.pause = true;

			if (cutscenes.playing) {
				didPauseCutscenes = true;
				cutscenes.Pause();
			}
			else {
				didPauseCutscenes = false;
			}

			
			var pausables = FindObjectsOfType<MonoBehaviour>().OfType<IPausable>();
			foreach (IPausable pausable in pausables) 
			{
				pausable.Pause();
			}
			
			float bgm = _bgmVolume.normalizedValue = (float) Settings.Instance.FetchSetting(Constants.BGM_Key, Settings.Type.Float);
			float sfx = _sfxVolume.normalizedValue = (float) Settings.Instance.FetchSetting(Constants.SFX_Key, Settings.Type.Float);

			didLoad = true;
		}

		protected override void DidClose()
		{
			didLoad = false;
			opacity.Hide();
			
			AudioListener.pause = false;
			
			var pausables = FindObjectsOfType<MonoBehaviour>().OfType<IPausable>();
			foreach (IPausable pausable in pausables) 
			{
				pausable.Resume();
			}

			if (didPauseCutscenes) {
				cutscenes.Play();
			}
		}
		
		#endregion
		
		#region Teleportation

		public void ToggleTeleport(bool active)
		{
			teleporter.SetActive(active);
			exitTextElement.text = string.Format("{0}. exit", (active) ? "iii" : "ii");
		}
		
		#endregion
		
		#region Ops

		public void ReturnToMainMenu()
		{
			StartCoroutine("ReturningToMainMenu");
			disposeInProgress = true;
		}
// Pebble63
		IEnumerator ReturningToMainMenu()
		{
			opacity.Hide();
			while (opacity.Visible) 
				yield return null;
			
			sceneAudio.FadeOut();
			SceneLoader.Instance.GoToScene(0);
		}

		public void Escape()
		{
			StartCoroutine("Exiting");
			disposeInProgress = true;
		}
		
		IEnumerator Exiting()
		{
			opacity.Hide();
			while (opacity.Visible) 
				yield return null;
			
			#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		
			 System.Diagnostics.Process.GetCurrentProcess().Kill();
			
			#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
			
			Application.Quit();
			
			#endif
		}

		public void Cancel()
		{
			if (disposeInProgress) return;
			
			Close();
		}

		public void Teleport()
		{
			StartCoroutine("Teleporting");
			disposeInProgress = true;
		}

		IEnumerator Teleporting()
		{
			opacity.Hide();
			while (opacity.Visible) 
				yield return null;
			
			sceneAudio.FadeOut();
			SceneLoader.Instance.GoToScene(teleportSceneIndex);
		}

		public void DidUpdateSettings()
		{
			if (!didLoad) return;
			
			Settings.Instance.ApplySetting(Constants.BGM_Key, Settings.Type.Float, _bgmVolume.normalizedValue);
			Settings.Instance.ApplySetting(Constants.SFX_Key, Settings.Type.Float, _sfxVolume.normalizedValue);
		}
		
		#endregion
	}
}