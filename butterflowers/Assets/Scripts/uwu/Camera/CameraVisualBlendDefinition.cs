using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Camera Visual Blend Def", menuName = "Internal/Camera/Blend", order = 52)]
public class CameraVisualBlendDefinition : ScriptableObject
{
    public enum Type 
    {
        Opacity,
        Animation
    }

    public Type type = Type.Opacity;
    public AnimationClip animation;
}
