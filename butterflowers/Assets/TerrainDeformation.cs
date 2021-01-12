using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using butterflowersOS.Core;

public class TerrainDeformation : MonoBehaviour
{

    public Material mat;
    public string property = "PlayerPos";
    public Wand wand;
    public Vector4 wandPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wandPos = wand.wandShaderParam;
        mat.SetVector(property, wand.wandShaderParam);
    }
}
