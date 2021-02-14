using UnityEngine;

namespace uwu.UI.Elements
{
	[ExecuteInEditMode]
	public class Exit : MonoBehaviour
	{
		[SerializeField] RectTransform icon = null;
		readonly Vector2 anchor = new Vector2(64f, -64f);

		readonly Vector2 pivot = new Vector2(0f, 1f);
		RectTransform rect;

		readonly float size = 36f;

		void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		// Update is called once per frame
		void Update()
		{
			if (rect == null) return;

			if (rect.pivot != pivot)
				rect.pivot = pivot;

			if (rect.anchoredPosition != anchor)
				rect.anchoredPosition = anchor;

			if (icon == null) return;

			var sz = Vector2.one * size;
			if (icon.sizeDelta != sz)
				icon.sizeDelta = sz;
		}
	}
}