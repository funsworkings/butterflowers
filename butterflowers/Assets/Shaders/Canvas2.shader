Shader "Custom/Unlit/Canvas2"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        
        _TextureA ("Texture A", 2D) = "white" {}
        _TextureB ("Texture B", 2D) = "white" {}
        
        _TextureAStrength ("Texture A Strength", Range(0,1)) = 0.5
        _TextureBStrength ("Texture B Strength", Range(0,1)) = 0.5
    
        _TextureCount ("Texture Count", int) = 0
        
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.5
        _LerpSpeed ("Lerp Speed", Float) = 1.0

        _Death("Death", Range(0, 1)) = 0.0
        _DeathColor ("Death Color", Color) = (0,0,0,1)

        _Interval ("Interval", Float) = 0.0
        
        _DebugColor ("Debug Color", Color) = (1,1,1,1)
        _DebugStrength ("Debug Strength", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            
            UNITY_DECLARE_TEX2DARRAY(_Textures);
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            
            float4 _MainTex_ST;
            
            int _TextureCount;
            float _NoiseStrength;

            sampler2D _TextureA, _TextureB;
            float _TextureAStrength, _TextureBStrength;

            float _Death;
            fixed4 _DeathColor;
            
            fixed4 _DebugColor;
            float _DebugStrength;
            float _LerpSpeed;

            float _Interval;
         
            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }
            
            
            float2 FlowUV (float2 uv, float2 flowVector, float time) {
                return uv - ((flowVector + float2(time, time)) * _NoiseStrength);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 uv = float3(1.0, 1.0, 1.0);
                
                uv.xy = i.uv.xy;
                
                float2 dir = (tex2D(_NoiseTex, uv.xy)).rg;
                uv.xy = FlowUV(uv.xy, dir, _Interval);
                
                fixed4 col = fixed4(1.0, 1.0, 1.0, 1.0);

                if(_TextureCount > 0)
                    col = tex2D(_TextureA, uv)*_TextureAStrength + tex2D(_TextureB, uv)*_TextureBStrength;

                // apply fog
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return (1.0 - _Death)*col + _DeathColor;
                
                //return (1.0 - _DebugStrength)*ct + _DebugStrength*_DebugColor;
            }
            ENDCG
        }
    }
}
