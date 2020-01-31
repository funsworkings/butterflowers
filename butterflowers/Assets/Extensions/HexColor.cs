using System;
using UnityEngine;

[Serializable]
public class HexColor {
    public string color;

    public HexColor(){
        color = "#000000";
    }

    public HexColor(string color){
        this.color = color;
    }
}