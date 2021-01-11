using System;
using butterflowersOS.Core;
using UnityEngine;
using uwu.Snippets.Load;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class PauseMenu : GenericMenu
	{
		// External

		World World;
		
		// Properties

		ToggleOpacity opacity;

		
		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		protected override void Start()
		{
			Close();
		}

		void Update()
		{
			if (Input.GetKeyUp(KeyCode.Escape)) 
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
			SceneLoader.Instance.GoToScene(0);
		}

		public void Escape()
		{
			Application.Quit();
		}

		public void Cancel()
		{
			Close();
		}
		
		#endregion
	}
}