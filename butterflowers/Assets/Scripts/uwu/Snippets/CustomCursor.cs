﻿using System;
using System.Linq;
using UnityEngine;
using NativeCursor = UnityEngine.Cursor;

namespace uwu.Snippets
{
	public class CustomCursor : MonoBehaviour
	{
		public enum State
		{
			Normal,
			Hover,
			Remote
		}

		public State state = State.Normal;

		[SerializeField] Setting[] settings;

		private void OnApplicationFocus(bool hasFocus)
		{
			NativeCursor.visible = hasFocus;
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			NativeCursor.visible = !pauseStatus;
		}

		// Update is called once per frame
		void Update()
		{
			var settings = this.settings.Where(s => s.state == state);
			if (settings.Count() > 0) {
				var setting = settings.ElementAt(0);
				var tex = setting.icon;

				var mode = CursorMode.Auto;

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
                mode = CursorMode.ForceSoftware;
#endif

				var visible = NativeCursor.visible = (tex != null);
				if(visible)
					NativeCursor.SetCursor(tex, Vector2.zero, mode);
			}
		}

		[Serializable]
		public struct Setting
		{
			public State state;
			public Texture2D icon;
		}
	}
}