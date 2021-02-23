using System;
using System.Collections;
using System.Collections.Generic;
using butterflowersOS.Interfaces;
using UnityEngine;

[Obsolete("Obsolete API!", true)]
public class ColorWithTimeOfDay : MonoBehaviour, IReactToSun
{
    [SerializeField] Material material = null;
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
