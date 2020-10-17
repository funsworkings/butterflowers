using System;
using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Palette", menuName = "Settings/Colors/Palette", order = 52)]
	public class Palette : Global<ColorSetting, Color>
	{
		public enum Style
		{
			None,
			Default
		}

		public Color FetchColorFromSetting(Style style, float opacity = 1f)
		{
			var c = Color.black;

			var key = Enum.GetName(typeof(Style), style);
			c = GetValueFromKey(key);
			c.a = Mathf.Clamp01(opacity);

			return c;
		}
	}
}