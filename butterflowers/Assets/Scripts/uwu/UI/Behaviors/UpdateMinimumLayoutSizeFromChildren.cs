using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uwu.UI.Behaviors
{
	[RequireComponent(typeof(LayoutElement))]
	[ExecuteInEditMode]
	public class UpdateMinimumLayoutSizeFromChildren : MonoBehaviour
	{
		[SerializeField] bool width, height;

		public bool refresh = true;
		public bool continuous;
		RectTransform[] childElements;
		LayoutElement layoutElement;
		LayoutGroup layoutGroup;

		RectTransform rectTransform;

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			layoutElement = GetComponent<LayoutElement>();
			layoutGroup = GetComponent<LayoutGroup>();
		}


		void Start()
		{
			refresh = false;
			UpdateLayoutElementHeightFromChildren();
		}

		void Update()
		{
			if (Application.isPlaying) {
				if (refresh || continuous) {
					UpdateLayoutElementHeightFromChildren();
					refresh = false;
				}
			}
			else if (Application.isEditor) {
				UpdateLayoutElementHeightFromChildren();
				refresh = false;
			}
		}

		void UpdateLayoutElementHeightFromChildren()
		{
			if (layoutElement == null)
				layoutElement = GetComponent<LayoutElement>();

			if (layoutGroup == null)
				layoutGroup = GetComponent<LayoutGroup>();

			var children = new List<RectTransform>();
			for (var i = 0; i < transform.childCount; i++) {
				var rect = transform.GetChild(i).GetComponent<RectTransform>();
				if (rect != null)
					children.Add(rect);
			}

			childElements = children.ToArray();

			var index = 0;

			if (width) {
				var w = 0f;

				var hzlayout = layoutGroup != null ? layoutGroup as HorizontalLayoutGroup : null;

				foreach (var el in childElements) {
					if (el != rectTransform)
						w += el.sizeDelta.x;

					if (index++ > 0 && hzlayout != null)
						w += hzlayout.spacing;
				}

				if (layoutGroup != null) w += layoutGroup.padding.left + layoutGroup.padding.right;

				layoutElement.minWidth = w;
			}

			index = 0;
			if (height) {
				var h = 0f;

				var vtlayout = layoutGroup != null ? layoutGroup as VerticalLayoutGroup : null;

				foreach (var el in childElements) {
					if (el != rectTransform)
						h += el.sizeDelta.y;

					if (index++ > 0 && vtlayout != null)
						h += vtlayout.spacing;
				}

				if (layoutGroup != null) h += layoutGroup.padding.top + layoutGroup.padding.bottom;

				layoutElement.minHeight = h;
			}
		}
	}
}