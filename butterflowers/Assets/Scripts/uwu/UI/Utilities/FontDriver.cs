using System;
using TMPro;
using UnityEngine;
using uwu.Settings;
using FontStyle = uwu.Settings.Types.FontStyle;
using Setting = uwu.Settings.Types.FontStyle.Style;

namespace uwu.UI.Utilities
{
	[ExecuteInEditMode]
	public class FontDriver : MonoBehaviour
	{
		public enum Type
		{
			Empty,
			Native,
			TMP
		}

		public Setting setting = Setting.Body2;

		[SerializeField] FontStyle styling;

		Local settings;

		UnityEngine.UI.Text text;
		TMP_Text tMP_Text;
		Type type = Type.Empty;

		void Awake()
		{
			tMP_Text = GetComponent<TMP_Text>();
			if (tMP_Text == null) {
				text = GetComponent<UnityEngine.UI.Text>();
				if (text == null)
					type = Type.Empty; // Disable component if no component available
				else
					type = Type.Native;
			}
			else {
				type = Type.TMP;
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (type == Type.Empty || styling == null) return;

			var key = Enum.GetName(typeof(Setting), setting);
			var format = styling.GetValueFromKey(key);

			if (format == null) {
				format = new FontFormat();
				format.weight = FontFormat.Weight.Bold;
				format.size = 64f;
				return;
			}

			var size = format.size;
			var weight = format.weight;


			if (type == Type.Native) {
				if (!Application.isPlaying)
					if (text == null)
						text = GetComponent<UnityEngine.UI.Text>();

				if (text == null)
					return;

				text.font = styling.font;

				text.fontSize = (int) size;
				text.fontStyle = GetNativeFontWeight(weight);
			}
			else if (type == Type.TMP) {
				if (!Application.isPlaying)
					if (tMP_Text == null)
						tMP_Text = GetComponent<TMP_Text>();

				if (tMP_Text == null)
					return;

				tMP_Text.font = styling.tmpfont;

				tMP_Text.fontSize = size;
				tMP_Text.fontStyle = GetTMPFontWeight(weight);
			}
		}

		FontStyles GetTMPFontWeight(FontFormat.Weight weight)
		{
			if (weight == FontFormat.Weight.Bold)
				return FontStyles.Bold;

			return FontStyles.Normal;
		}

		UnityEngine.FontStyle GetNativeFontWeight(FontFormat.Weight weight)
		{
			if (weight == FontFormat.Weight.Bold)
				return UnityEngine.FontStyle.Bold;

			return UnityEngine.FontStyle.Normal;
		}
	}
}