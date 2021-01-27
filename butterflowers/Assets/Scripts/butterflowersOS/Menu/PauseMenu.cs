﻿using System;
using System.Collections;
using butterflowersOS.Core;
using UnityEngine;
using uwu.Snippets.Load;
using uwu.Timeline.Core;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class PauseMenu : GenericMenu
	{
		// External

		World World;

		[SerializeField] Cutscenes cutscenes;
		
		// Properties

		ToggleOpacity opacity;

		bool disposeInProgress = false;
		public bool Dispose => disposeInProgress;
		
		
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
			if (Input.GetKeyUp(KeyCode.Escape) && World.LOAD && !disposeInProgress) 
				Toggle();
		}

		#region Menu

		protected override void DidOpen()
		{
			opacity.Show();
		}

		protected override void DidClose()
		{
			opacity.Hide();
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
		
			Application.Quit();
		}

		public void Cancel()
		{
			if (disposeInProgress) return;
			
			Close();
		}
		
		#endregion
	}
}