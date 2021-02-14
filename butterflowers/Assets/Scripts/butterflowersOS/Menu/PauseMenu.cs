using System;
using System.Collections;
using butterflowersOS.Core;
using TMPro;
using UnityEngine;
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

		bool disposeInProgress = false;
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
			if (Input.GetKeyUp(KeyCode.Escape) && !cutscenes.playing &&  !disposeInProgress) 
				Toggle();
		}

		void OnDestroy()
		{
			AudioListener.pause = false; // Discard audio listener changess
		}

		#region Menu

		protected override void DidOpen()
		{
			opacity.Show();
			AudioListener.pause = true;
		}

		protected override void DidClose()
		{
			opacity.Hide();
			AudioListener.pause = false;
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
		
			if (!Application.isEditor) { System.Diagnostics.Process.GetCurrentProcess().Kill(); }
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
		
		#endregion
	}
}