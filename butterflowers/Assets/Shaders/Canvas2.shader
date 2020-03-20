Shader "Custom/Unlit/Canvas2"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
    
        _Textures ("Textures", 2DArray) = ""{}
        _TextureCount ("Texture Count", int) = 0
        
        _TextureStrength ("Texture Strength", Range(0,1)) = 0.5
        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.5
        
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
            float _TextureStrength, _NoiseStrength;
            
            fixed4 _DebugColor;
            float _DebugStrength;
         
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
                uv.xy = FlowUV(uv.xy, dir, _Time.y);
                
                float t = _Time.y;
                float str = 0.0;

                fixed4 mid = fixed4(.5, .5, .5, .5);
                fixed4 ct = fixed4(1.0, 1.0, 1.0, 1.0);
                
                fixed4 c = fixed4(1.0, 1.0, 1.0, 1.0);
                for(int i = 0; i < _TextureCount; i++){
                    uv.z = i;
                
                    c = UNITY_SAMPLE_TEX2DARRAY(_Textures, uv);
                    
                    str = _TextureStrength*(( sin( t + (i*1.0)/_TextureCount*6.28 )));
                    ct += ((c)*str);
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, ct);
                return (1.0 - _DebugStrength)*ct + _DebugStrength*_DebugColor;
            }
            ENDCG
        }
    }
}
