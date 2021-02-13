using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyMenuPropertyBlock : MonoBehaviour
{

    MaterialPropertyBlock propBlock;
    Renderer r;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        propBlock.SetFloat("_TimeOffset", Random.Range(0f,1f));
        propBlock.SetFloat("_Speed", Random.Range(0f, .067f));
        r.SetPropertyBlock(propBlock);
    }

}
