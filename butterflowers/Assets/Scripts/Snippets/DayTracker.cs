using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DayTracker : MonoBehaviour, IReactToSun
{
    Text text;

    public void ReactToDay(int days)
    {
        text.text = days + "";
    }

    public void ReactToTimeOfDay(float timeOfDay)
    {
        return;
    }

    void Awake()
    {
        text = GetComponent<Text>();
    }


}
