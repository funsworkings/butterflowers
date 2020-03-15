Shader "Custom/Unlit/Canvas2"
{
    Properties
    {
        _Textures ("Textures", 2DArray) = ""{}
        _TextureCount ("Texture Count", int) = 0
        
        _TextureStrength ("Texture Strength", Range(0,1)) = 0.5
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
            
            int _TextureCount;
            float _TextureStrength;
         
            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = o.vertex / 2.0;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 uv = float3(1.0, 1.0, 1.0);
                
                uv.xy = i.uv.xy;
                

                fixed4 mid = fixed4(.5, .5, .5, .5);
                fixed4 ct = fixed4(1.0, 1.0, 1.0, 1.0);
                
                fixed4 c = fixed4(1.0, 1.0, 1.0, 1.0);
                for(int i = 0; i < _TextureCount; i++){
                    uv.z = i;
                
                    c = UNITY_SAMPLE_TEX2DARRAY(_Textures, uv);
                    ct += ((c - mid)*_TextureStrength);
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, ct);
                return ct;
            }
            ENDCG
        }
    }
}
