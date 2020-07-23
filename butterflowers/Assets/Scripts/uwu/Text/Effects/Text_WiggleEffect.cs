using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_WiggleEffect : TextEffect
{
    public float magnitude = 1f;

    [SerializeField] float wavelength = 6.28f;
    [SerializeField] float speed = 1f;


    protected override void ComputeTranslationPerVertex(ref Vector3 offset, int index, int length)
    {
        float interval = (float)index / 10f;
        float period = (2f * Mathf.PI / wavelength);

        float len = magnitude * Mathf.Sin((Time.time * speed + (interval * wavelength )) / period);
        offset = Vector3.up * len;
    }
}
