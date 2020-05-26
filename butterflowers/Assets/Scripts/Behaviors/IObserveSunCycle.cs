using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserveSunCycle
{
    void onDayBegin();
    void onDayEnd();

    void onNightBegin();
    void onNightEnd();
}
