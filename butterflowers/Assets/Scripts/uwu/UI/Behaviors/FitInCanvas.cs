using UnityEngine;

namespace uwu.UI.Behaviors
{
	// todo!!!!!!!!!!!!
	[RequireComponent(typeof(RectTransform))]
	public class FitInCanvas : MonoBehaviour
	{
		[SerializeField] float paddingTop, paddingBottom;
		[SerializeField] float paddingLeft, paddingRight;

		Canvas canvas;
		float height = -1f;
		RectTransform rectTransform;

		float width = -1f;

		void Awake()
		{
			canvas = GetComponentInParent<Canvas>();
			rectTransform = GetComponent<RectTransform>();
		}

		void Start()
		{
			width = rectTransform.sizeDelta.x;
			height = rectTransform.sizeDelta.y;
		}

		void LateUpdate()
		{
			var rect = rectTransform.rect;

			var w = rectTransform.sizeDelta.x;
			var h = rectTransform.sizeDelta.y;

			var cw = canvas.pixelRect.width / 2f;
			var ch = canvas.pixelRect.height / 2f;

			var pos = rectTransform.anchoredPosition;

			var left = pos.x - width / 2f;
			var right = pos.x + width / 2f;
			var top = pos.y + height / 2f;
			var bott = pos.y - height / 2f;

			var wOffset = -(cw + left - paddingLeft) + (cw - right - paddingRight);
			var hOffset = -(ch + bott - paddingBottom) + (ch - top - paddingTop);

			Debug.LogFormat("woff = {0} hoff = {1}", wOffset, hOffset);

			w = Mathf.Clamp(w + wOffset, 0f, width);
			h = Mathf.Clamp(h + hOffset, 0f, height);

			rectTransform.sizeDelta = new Vector2(w, h);
		}
	}
}