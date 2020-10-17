using UnityEngine;
using UnityEngine.UI;

namespace uwu.UI.Elements
{
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectNavigator : MonoBehaviour
	{
		[SerializeField] float moveMultiplier = 1f;

		ScrollRect scroll;

		public RectTransform content => scroll.content;

		public float width => content.sizeDelta.x;
		public float height => content.sizeDelta.y;

		public int items => content.childCount;

		void Awake()
		{
			scroll = GetComponent<ScrollRect>();
		}

		#region Operations

		public void MoveUpUnit()
		{
			var amount = items > 0 ? height / items : 1f;
			Move(Vector2.up * amount);
		}

		public void MoveDownUnit()
		{
			var amount = items > 0 ? height / items : 1f;
			Move(Vector2.down * amount);
		}

		public void Move(Vector2 amount)
		{
			scroll.normalizedPosition += new Vector2(amount.x / width, amount.y / height) * moveMultiplier;
		}

		public void MoveToTop()
		{
			scroll.normalizedPosition = new Vector2(0, 1);
		}

		public void MoveToBottom()
		{
			scroll.normalizedPosition = new Vector2(0, 0);
		}

		#endregion
	}
}