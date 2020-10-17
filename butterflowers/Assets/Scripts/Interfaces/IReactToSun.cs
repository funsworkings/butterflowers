using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReactToSun
{
    void ReactToTimeOfDay(float timeOfDay);
    void ReactToDay(int days);
}
