using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoemGeometryMaterialPropBlock : MonoBehaviour
{

    [Header("Destruction Variables")]
    [Space(10)]
    [SerializeField] float maskClip_d;
    [SerializeField] float interactionRadius_d;
    [SerializeField] float size;
    [SerializeField] float hardness_d;
    [SerializeField] float displacement;
    [SerializeField] float vertexNormalStrength;
    [SerializeField] float vertexOffsetStrength;
    [SerializeField] float smoothness;
    [SerializeField] Texture2D flowTexture, vertexOffsetMask, roundedEmission, opacityMask, materialTexture, normal;
    [SerializeField] Vector2 emissionTiling, maskTiling, materialTiling;

    [Header("Nurture Variables")]
    [Space(10)]
    [SerializeField] float gridUnits;
    [SerializeField] float interactionRadius_n;
    [SerializeField] float maskClip_n;
    [SerializeField] float hardness_n;
    [SerializeField] float displacement_n;
    [SerializeField] float radialAmount;
    [SerializeField] float smoothness_n;
    [SerializeField] float opacity;
    [SerializeField] Color emptyColor, fullColor;
    [SerializeField] Texture2D albedoTex, emissionTex, normalTex;

    [Header("Quiet Variables")]
    [Space(10)]
    [SerializeField] float smoothness_q;
    [SerializeField] float normal_height;
    [ColorUsageAttribute(true, true)]
    [SerializeField] Color albedoCol, emissonCol;
    [SerializeField] Texture2D albedo_q, metallic, normal_q;
    [SerializeField] Vector2 albedoTiling_q;
    
    [Header("Order Variables")]
    [Space(10)]
    [SerializeField] float maskClip_o;
    [SerializeField] float interactionRadius_o;
    [SerializeField] float size_o;
    [SerializeField] float hardness_o;
    [SerializeField] float worldNoise_o;
    [SerializeField] float baseFlip;
    [SerializeField] float displacement_o;
    [SerializeField] float interactionStrength_o;
    [SerializeField] Texture2D texture;
    [SerializeField] Vector2 textureTiling;

    MaterialPropertyBlock propertyBlock;
    Renderer rend;
    
    void Awake()
    {
        rend = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        
        //destruction
        if (gameObject.layer == 30) {
            Debug.Log("Set property block for north star material");
            propertyBlock.SetFloat("_Cutoff", maskClip_d);
            propertyBlock.SetFloat("_InteractionRadius", interactionRadius_d);
            propertyBlock.SetFloat("_Size", size);
            propertyBlock.SetFloat("_Hardness", hardness_d);
            propertyBlock.SetFloat("_Displacement", displacement);
            propertyBlock.SetFloat("_VertexNormalStrength", vertexNormalStrength);
            propertyBlock.SetFloat("_VertexOffsetStrength", vertexOffsetStrength);
            propertyBlock.SetFloat("_Smoothness", smoothness);

            if(flowTexture != null)
            propertyBlock.SetTexture("_Flow", flowTexture);

            if(vertexOffsetMask != null)
            propertyBlock.SetTexture("_VertexOffsetMask", vertexOffsetMask);

            if(roundedEmission != null)
            propertyBlock.SetTexture("_RoundedEmission", roundedEmission);

            if(opacityMask != null)
            propertyBlock.SetTexture("_OpacityMask", opacityMask);

            if(materialTexture != null)
            propertyBlock.SetTexture("_MaterialTexture", materialTexture);

            if(normal != null)
            propertyBlock.SetTexture("_Normal", normal);

            propertyBlock.SetVector("_RoundedEmission_ST", new Vector4(emissionTiling.x, emissionTiling.y));
            propertyBlock.SetVector("_OpacityMask_ST", new Vector4(maskTiling.x, maskTiling.y));
            propertyBlock.SetVector("_MaterialTexture_ST", new Vector4(materialTiling.x, materialTiling.y));

        }

        //nurture
        if (gameObject.layer == 27)
        {

            Debug.Log(name + "is setting prop block for material");
            propertyBlock.SetFloat("_GridUnits", gridUnits);
            propertyBlock.SetFloat("_InteractionRadius", interactionRadius_n);
            propertyBlock.SetFloat("_Cutoff", maskClip_n);
            propertyBlock.SetFloat("_hardness", hardness_n);
            propertyBlock.SetFloat("_Displacement", displacement_n);
            propertyBlock.SetFloat("_RadialAmount", radialAmount);
            propertyBlock.SetFloat("_Smoothness", smoothness_n);
            propertyBlock.SetFloat("_Opacity", opacity);

            propertyBlock.SetColor("_EmptyColor", emptyColor);
            propertyBlock.SetColor("_FullColor", fullColor);

            if(albedoTex != null)
            propertyBlock.SetTexture("_FillTex", albedoTex);

            if(emissionTex != null)
            propertyBlock.SetTexture("_Emission", emissionTex);

            if(normalTex != null)
            propertyBlock.SetTexture("_Normal", normalTex);

        }

        //quiet
        if(gameObject.layer == 31)
        {
            propertyBlock.SetFloat("_GlossMapScale", smoothness_q);
            propertyBlock.SetFloat("_BumpScale", normal_height);

            propertyBlock.SetColor("_Color", albedoCol);
            propertyBlock.SetColor("_EmissionColor", emissonCol);

            if(albedo_q != null)
            propertyBlock.SetTexture("_MainTex", albedo_q);

            if(metallic != null)
            propertyBlock.SetTexture("_MetallicGlossMap", metallic);

            if(normal_q != null)
            propertyBlock.SetTexture("_BumpMap", normal_q);

            propertyBlock.SetVector("_MainTex_ST", new Vector4(albedoTiling_q.x, albedoTiling_q.y));
        }

        //order
        if(gameObject.layer == 28)
        {
            propertyBlock.SetFloat("_Cutoff", maskClip_o);
            propertyBlock.SetFloat("_InteractionRadius", interactionRadius_o);
            propertyBlock.SetFloat("_Size", size_o);
            propertyBlock.SetFloat("_hardness", hardness_o);
            propertyBlock.SetFloat("_WorldNoiseStrength", worldNoise_o);
            propertyBlock.SetFloat("_BaseFlip", baseFlip);
            propertyBlock.SetFloat("_Displacement", displacement_o);
            propertyBlock.SetFloat("_InteractionStrength", interactionStrength_o);

            if(texture != null)
            propertyBlock.SetTexture("_treetop_1", texture);

            propertyBlock.SetVector("_treetop_1_ST", new Vector4(textureTiling.x, textureTiling.y));
        }

        rend.SetPropertyBlock(propertyBlock);
    }
}
