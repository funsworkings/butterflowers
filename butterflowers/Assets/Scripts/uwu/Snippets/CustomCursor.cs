using System;
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
		public bool lockCursor = false;

		[SerializeField] Setting[] settings = new Setting[]{};

		void Start()
		{
			NativeCursor.lockState = (lockCursor) ? CursorLockMode.Locked : CursorLockMode.None;
		}

		// Update is called once per frame
		void Update()
		{
			Texture2D t_texture = null;

			if (!lockCursor) 
			{
				var settings = this.settings.Where(s => s.state == state);
				if (settings.Count() > 0) {
					var setting = settings.ElementAt(0);
					t_texture = setting.icon;
				}
			}

			NativeCursor.SetCursor(t_texture, Vector2.zero, CursorMode.Auto);
		}

		[Serializable]
		public struct Setting
		{
			public State state;
			public Texture2D icon;
		}
	}
}