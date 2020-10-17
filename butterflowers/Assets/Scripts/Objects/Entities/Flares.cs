using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flares : MonoBehaviour
{
    RawImage rawImage;
    Material mat;

    float brightness = 0f, t_brightness = 1f;

    [SerializeField] float minBrightness, maxBrightness = 1f;
    [SerializeField] float fadeSpeed = 1f;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Start()
    {
        mat = rawImage.material;
        brightness = t_brightness = minBrightness;
    }

    void Update()
    {
        Fade();
    }

    public void Show()
    {
        t_brightness = maxBrightness;
    }

    void Fade()
    {
        if (brightness == maxBrightness) 
            t_brightness = minBrightness;

        float diff = Mathf.Abs(t_brightness - brightness);
        if (diff > .1f) {
            brightness = Mathf.Lerp(brightness, t_brightness, Time.deltaTime * fadeSpeed);
        }
        else
            brightness = t_brightness;

        mat.SetFloat("_Threshold", brightness);
    }
}
