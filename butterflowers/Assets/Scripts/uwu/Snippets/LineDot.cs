using UnityEngine;

namespace uwu.Snippets
{
	[ExecuteInEditMode]
	public class LineDot : MonoBehaviour
	{
		public float x, y, radius;
		RectTransform rect;

		void Update()
		{
			if (rect == null)
				rect = GetComponent<RectTransform>();

			rect.anchoredPosition = new Vector2(x, y);
			rect.sizeDelta = Vector2.one * (radius / 2f);
		}
	}
}