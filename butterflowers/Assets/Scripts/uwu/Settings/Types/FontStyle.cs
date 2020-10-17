using TMPro;
using UnityEngine;
using uwu.Settings.Instances;

namespace uwu.Settings.Types
{
	[CreateAssetMenu(fileName = "New Font Style", menuName = "Settings/Fonts/Style", order = 52)]
	public class FontStyle : Global<FontSetting, FontFormat>
	{
		public enum Style
		{
			//H1, H2, H3, H4, Body, Small, H5,

			//new
			ItemHeader,
			Header1,
			Header2,
			Subheader1,
			Body1,
			Body2,
			Notification,
			Search
		}

		public Font font;
		public TMP_FontAsset tmpfont;
	}
}