using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Obsolete API!", true)]
[CreateAssetMenu(fileName = "New Gesture", menuName = "Extras/Gesture", order = 52)]
public class Gesture : ScriptableObject
{
    public AnimationClip clip;

    [Range(0f, 1f)]
    public float mood;
}
