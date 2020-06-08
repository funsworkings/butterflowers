using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flares : MonoBehaviour
{
    LensFlare flare;

    [SerializeField] float brightness = 0f, t_brightness = 1f;
    [SerializeField] float fadeSpeed = 1f;

    void Awake()
    {
        flare = GetComponent<LensFlare>();
    }

    void OnEnable()
    {
        Discovery.onDiscoverNew += Show;
    }

    void OnDisable()
    {
        Discovery.onDiscoverNew -= Show;
    }

    void Update()
    {
        Fade();
    }

    void Show()
    {
        t_brightness = 1f;
    }

    void Fade()
    {
        if (brightness == 1f) 
            t_brightness = 0f;

        float diff = Mathf.Abs(t_brightness - brightness);
        if (diff > .1f) {
            brightness = Mathf.Lerp(brightness, t_brightness, Time.deltaTime * fadeSpeed);
        }
        else
            brightness = t_brightness;

        flare.color = Color.HSVToRGB(0f, 0f, Mathf.Clamp01(brightness));
    }
}
