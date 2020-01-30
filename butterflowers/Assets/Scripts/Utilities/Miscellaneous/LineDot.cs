using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class LineDot : MonoBehaviour {
    RectTransform rect;

    public float x = 0f, y = 0f, radius = 0f;

    void Update() {
        if(rect == null)
            rect = GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = Vector2.one * (radius / 2f);    
    }

}