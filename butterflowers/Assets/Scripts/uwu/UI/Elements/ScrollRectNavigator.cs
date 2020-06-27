using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIExt.Elements {

    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectNavigator: MonoBehaviour {

        ScrollRect scroll;

        [SerializeField] float moveMultiplier = 1f;

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
            float amount = (items > 0)? (height / items):1f;
            Move(Vector2.up * amount);
        }

        public void MoveDownUnit()
        {
            float amount = (items > 0)? (height / items):1f;
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
