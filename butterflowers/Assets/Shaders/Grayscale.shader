Shader "Hidden/Custom/Grayscale"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
    
        float _Blend;
        float _Intensity;
        float _Brightness;
        float _Offset;
        float _Tiling;
        float4 _Color;
 
        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float2 texcoord = fmod(i.texcoord / _Tiling, float2(1, 1));
            //texcoord = i.texcoord;
            float2 s_texcoord = fmod(i.texcoordStereo / _Tiling, float2(1, 1));
            //s_texcoord = i.texcoordStereo;
            
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord);
            float depth = min(1, (LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, s_texcoord)) / _Offset) + _Brightness);

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