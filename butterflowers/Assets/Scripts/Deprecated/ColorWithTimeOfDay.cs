﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWithTimeOfDay : MonoBehaviour, IReactToSun
{
    [SerializeField] Material material;
    [SerializeField] Color baseColor = new Color(1f, 1f, 1f, 1f);

    [SerializeField] bool inverted = false;

    public void ReactToDay(int days)
    {
        return;
    }

    public void ReactToTimeOfDay(float timeOfDay)
    {
        if (material == null)
            return;

        float value = (inverted) ? (1f - timeOfDay) : timeOfDay;
        Color col = baseColor * value;

        material.color = new Color(col.r, col.g, col.b, baseColor.a);
    }
}
