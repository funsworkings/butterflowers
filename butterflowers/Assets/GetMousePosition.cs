using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMousePosition : MonoBehaviour
{
    public Material lava;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lava.SetVector("_MousePos", Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
}
