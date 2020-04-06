using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithTimeOfDay : MonoBehaviour, IReactToSun
{
    [SerializeField] Vector3 axis;
    [SerializeField] float offset = 0f;

    public void ReactToDay(int days)
    {
        return;
    }

    public void ReactToTimeOfDay(float timeOfDay)
    {
        transform.localRotation = Quaternion.AngleAxis((360f * (offset + timeOfDay)), axis);
    }
}
