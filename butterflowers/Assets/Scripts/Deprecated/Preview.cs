using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Preview : MonoBehaviour
{
    RectTransform rect;
    RawImage image;

    [SerializeField] CanvasGroup canvasGroup;

    public bool hiding = false;

    [SerializeField] float maxScale = 5f, scaleSpeed = 1f;
    [SerializeField] float fadeSpeed = 3f;

    void Awake() {
        rect = GetComponent<RectTransform>();
        image = GetComponent<RawImage>();    
    }

    void Start() {
         Mother.onSuccessLoadTexture += OnReceiveImage;  
    }

    void OnReceiveImage(Texture tex){
        if(hiding)
            StopCoroutine("Hiding");
        else {
            rect.localScale = Vector3.zero;
            canvasGroup.alpha = 1f;
        }

        hiding = true;
        StartCoroutine("Hiding");
    }

    IEnumerator Hiding(){
        float opacity = canvasGroup.alpha;
        Vector3 scale = rect.localScale;

        while(scale.magnitude < maxScale || opacity > 0f){
            var dt = Time.deltaTime;

            scale += (Vector3.one * scaleSpeed * dt);
            opacity -= (fadeSpeed * dt);

            canvasGroup.alpha = Mathf.Clamp01(opacity);
            rect.localScale = scale;

            yield return null;
        }

        hiding = false;
    }

    void OnDestroy() {
        Mother.onSuccessLoadTexture -= OnReceiveImage;  
    }
}
