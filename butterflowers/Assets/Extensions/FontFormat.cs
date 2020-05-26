using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FontFormat
{
    public float size = 1f;

    public enum Weight { Normal, Bold }
    public Weight weight = Weight.Normal;
}
