using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


namespace UIExt.Elements {
    [ExecuteInEditMode]
    public class Footer : MonoBehaviour
    {
        RectTransform rect;

        Vector2 pivot = new Vector2(.5f, 0f);
        Vector2 anchor = Vector2.zero;

        private void Awake() {
            rect = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if(rect == null) return;

            if(rect.pivot != pivot)
                rect.pivot = pivot;

            if(rect.anchoredPosition != anchor)
                rect.anchoredPosition = anchor;
        }
    }
}
