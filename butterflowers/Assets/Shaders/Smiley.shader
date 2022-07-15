Shader "Hidden/Custom/Smiley"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        
        Texture2D _Gradient, _HeatmapA, _HeatmapB;

        half _Steps;
        float _Blend, _HeatmapBlend;
    
        //float _Tiling;
        //float4 _Color;
 
        float4 Frag(VaryingsDefault i) : SV_Target
        {
            //float2 texcoord = fmod(i.texcoord / _Tiling, float2(1, 1));
            float2 texcoord = i.texcoord;
            //float2 s_texcoord = fmod(i.texcoordStereo / _Tiling, float2(1, 1));
            float2 s_texcoord = i.texcoordStereo;
            
            float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord);
            baseColor.rgb = floor(baseColor.rgb * _Steps) / _Steps;
                float4 gradient = SAMPLE_TEXTURE2D(_Gradient, sampler_MainTex, float2(length(baseColor.rgb), 0));
                float value = length(gradient.rgb);

            float4 heatmapAColor = SAMPLE_TEXTURE2D(_HeatmapA, sampler_MainTex, float2(value, 0));
            float4 heatmapBColor = SAMPLE_TEXTURE2D(_HeatmapB, sampler_MainTex, float2(value, 0));
                float4 heatmapFinalColor = lerp(heatmapAColor, heatmapBColor, _HeatmapBlend); // Blend heatmap
            
            baseColor.rgb = lerp(baseColor.rgb, heatmapFinalColor, _Blend);
            
            //float depth = min(1, (LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, s_texcoord)) / _Offset) + _Brightness);

            //float3 tColor = lerp(float3(depth, depth, depth), _Color.rgb * depth, _Blend.xxx);
            //color.rgb = lerp(color.rgb, tColor, _Intensity.xxx);
            
            return baseColor;
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