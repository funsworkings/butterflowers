using UnityEngine;

namespace butterflowersOS.Menu
{
	public class MainMenuSubPage : GenericMenu
	{
		// Properties

		protected MainMenu _controller;

		protected override void Start()
		{
			_controller = FindObjectOfType<MainMenu>();
			
			base.Start();
		}

		protected virtual void Update()
		{
			if (!IsVisible) return;
			
			if (Input.GetKeyUp(KeyCode.Escape)) 
			{
				Close();	
			}
		}

		protected override void DidOpen()
		{
			
		}

		protected override void DidClose()
		{
			_controller.Reset();
		}
	}
}