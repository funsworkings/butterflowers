using System;
using butterflowersOS.Utils;
using UnityEngine;
using UnityEngine.UI;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class SettingsMenu : GenericMenu
	{
		// Properties

		ToggleOpacity opacity;

		[SerializeField] MainMenu _mainMenu = null;
		[SerializeField] Slider _bgmVolume = null, _sfxVolume = null;

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		protected override void Start()
		{
			_bgmVolume.value = (float) Settings.Instance.FetchSetting("bgm_volume", Settings.Type.Float);
			_sfxVolume.value = (float) Settings.Instance.FetchSetting("sfx_volume", Settings.Type.Float);
			
			base.Start();
		}

		void Update()
		{
			if (!IsVisible) return;

			if (Input.GetKeyUp(KeyCode.Escape)) {
				_mainMenu.Reset();
			}
		}

		protected override void DidOpen()
		{
			opacity.Show();

			_bgmVolume.value = (float) Settings.Instance.FetchSetting("bgm_volume", Settings.Type.Float);
			_sfxVolume.value = (float) Settings.Instance.FetchSetting("sfx_volume", Settings.Type.Float);
		}

		protected override void DidClose()
		{
			opacity.Hide();

			Settings.Instance.ApplySetting("bgm_volume", Settings.Type.Float, _bgmVolume.value);
			Settings.Instance.ApplySetting("sfx_volume", Settings.Type.Float, _sfxVolume.value);
		}
	}
}