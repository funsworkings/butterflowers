using System;
using butterflowersOS.Utils;
using UnityEngine;
using UnityEngine.UI;
using uwu.UI.Behaviors.Visibility;

namespace butterflowersOS.Menu
{
	public class SettingsMenu : MainMenuSubPage
	{
		// Properties

		ToggleOpacity opacity;
		
		[SerializeField] Slider _bgmVolume = null, _sfxVolume = null;

		bool load = false;

		void Awake()
		{
			opacity = GetComponent<ToggleOpacity>();
		}

		protected override void Start()
		{
			_bgmVolume.normalizedValue = (float) Settings.Instance.FetchSetting("bgm_volume", Settings.Type.Float);
			_sfxVolume.normalizedValue = (float) Settings.Instance.FetchSetting("sfx_volume", Settings.Type.Float);
			
			base.Start();
		}

		protected override void DidOpen()
		{
			load = true;
			opacity.Show();

			_bgmVolume.normalizedValue = (float) Settings.Instance.FetchSetting("bgm_volume", Settings.Type.Float);
			_sfxVolume.normalizedValue = (float) Settings.Instance.FetchSetting("sfx_volume", Settings.Type.Float);
		}

		protected override void DidClose()
		{
			opacity.Hide();
			
			base.DidClose();
		}

		public void DidUpdate()
		{
			if (!load) return;
			
			Settings.Instance.ApplySetting("bgm_volume", Settings.Type.Float, _bgmVolume.normalizedValue);
			Settings.Instance.ApplySetting("sfx_volume", Settings.Type.Float, _sfxVolume.normalizedValue);
		}
	}
}