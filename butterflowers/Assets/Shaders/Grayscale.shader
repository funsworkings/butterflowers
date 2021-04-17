Shader "Hidden/Custom/Grayscale"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
    
        float _Blend;
        float _Intensity;
        float _Offset;
        float _Tiling;
        float4 _Color;

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord / _Tiling);
            float depth = min(1, LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo)) / _Offset);

            float3 tColor = lerp(float3(depth, depth, depth), _Color.rgb * depth, _Blend.xxx);
            color.rgb = lerp(color.rgb, tColor, _Intensity.xxx);
            
            return color;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}