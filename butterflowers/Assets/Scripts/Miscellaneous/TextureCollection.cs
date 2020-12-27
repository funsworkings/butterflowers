using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Texture Collection", menuName = "Extras/Texture Collection", order = 52)]
public class TextureCollection : ScriptableObject
{
    public Texture2D[] elements;
}
