using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoemGeometryMaterialPropBlock : MonoBehaviour
{


    [SerializeField] float maskClip, interactionRadius, size, hardness, displacement, vertexNormalStrength, vertexOffsetStrength, smoothness;
    [SerializeField] Texture2D flowTexture, vertexOffsetMask, roundedEmission, opacityMask, materialTexture, normal;
    [SerializeField] Vector2 flowTiling, vertexTiling, emissionTiling, maskTiling, materialTiling, normalTiling;

    MaterialPropertyBlock propertyBlock;
    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();


        if (gameObject.layer == 30) {
            propertyBlock.SetFloat("_Cutoff", maskClip);
            propertyBlock.SetTexture("_Flow", flowTexture);
            propertyBlock.SetVector("", flowTiling);

        }

        rend.SetPropertyBlock(propertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
