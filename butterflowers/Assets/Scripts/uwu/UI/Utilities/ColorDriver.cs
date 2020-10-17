using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uwu.Settings.Types;

namespace uwu.UI.Utilities
{
	[ExecuteInEditMode]
	public class ColorDriver : MonoBehaviour
	{
		public enum Type
		{
			Empty,
			Image,
			RawImage,
			Text,
			TMPText
		}

		public Palette.Style style = Palette.Style.Default;
		public Palette.Style defaultStyle = Palette.Style.Default;

		[Range(0, 1)] public float opacity = 1f;

		[SerializeField] Palette styling;
		bool defaulted = false;

		Image image;
		RawImage rawImage;

		UnityEngine.UI.Text text;
		TMP_Text tMP_Text;
		Type type = Type.Empty;


		void Awake()
		{
			image = GetComponent<Image>();
			if (image == null) {
				rawImage = GetComponent<RawImage>();
				if (rawImage == null) {
					tMP_Text = GetComponent<TMP_Text>();
					if (tMP_Text == null) {
						text = GetComponent<UnityEngine.UI.Text>();
						if (text == null)
							type = Type.Empty;
						else
							type = Type.Text;
					}
					else {
						type = Type.TMPText;
					}
				}
				else {
					type = Type.RawImage;
				}
			}
			else {
				type = Type.Image;
			}


			// if (Application.isPlaying)
			//{
			defaultStyle = style;
			//}
		}

		// Update is called once per frame
		void Update()
		{
			UpdateColor();
		}

		public void UpdateColor()
		{
			if (type == Type.Empty || styling == null) return;

			var color = styling.FetchColorFromSetting(style, opacity);

			if (type == Type.Image) {
				if (!Application.isPlaying)
					if (image == null)
						image = GetComponent<Image>();

				if (image == null)
					return;

				image.color = color;
			}
			else if (type == Type.RawImage) {
				if (!Application.isPlaying)
					if (rawImage == null)
						rawImage = GetComponent<RawImage>();

				if (rawImage == null)
					return;

				rawImage.material.color = color;
			}
			else if (type == Type.TMPText) {
				if (!Application.isPlaying)
					if (tMP_Text == null)
						tMP_Text = GetComponent<TMP_Text>();

				if (tMP_Text == null)
					return;

				tMP_Text.color = color;
			}
			else if (type == Type.Text) {
				if (!Application.isPlaying)
					if (text == null)
						text = GetComponent<UnityEngine.UI.Text>();

				if (text == null)
					return;

				text.color = color;
			}
		}

		public void SetColor(int _style)
		{
			style = (Palette.Style) _style;
		}

		public void ResetColor()
		{
			style = defaultStyle;
		}
	}
}