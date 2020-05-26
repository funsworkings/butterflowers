using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace UIExt.Elements {
    [ExecuteInEditMode]
    public class Exit : MonoBehaviour
    {
        RectTransform rect;
        [SerializeField] RectTransform icon;

        Vector2 pivot = new Vector2(0f, 1f);
        Vector2 anchor = new Vector2(64f, -64f);

        float size = 36f;

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

            if(icon == null) return;

            Vector2 sz = Vector2.one * size;
            if(icon.sizeDelta != sz)
                icon.sizeDelta = sz;
        }
    }
}
