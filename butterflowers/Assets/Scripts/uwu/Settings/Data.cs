using System;
using UnityEngine;

namespace uwu.Settings
{
	[Serializable]
	public class Data
	{
		[Header("Attributes")] public bool sfx = true;

		public bool music = true;

		public float sensitivity = 1f;
		public float brightness = 1f;
		public float textSize = 1f;

		public bool notifications;

		public Data()
		{
		}

		public Data(bool sfx, bool music, float sensitivity, float brightness, float text, bool notifications)
		{
			this.sfx = sfx;
			this.music = music;
			this.sensitivity = Mathf.Clamp01(sensitivity);
			this.brightness = Mathf.Clamp01(brightness);
			textSize = Mathf.Clamp01(text);
			this.notifications = notifications;
		}
	}
}