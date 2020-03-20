Shader "Custom/Butterfly"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        
        _Death ("Death", Range(0, 1)) = 0.0
        _DeathColor ("Death Color", Color) = (1,1,1,1)
        
        _Tiling ("Tiling", Float) = 1.0
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

            #include "UnityCG.cginc"
           

            struct v2f
            {
                float2 uv : TEXCOORD0;
            };
           
            half _Death;
        
            fixed4 _Color;
            fixed4 _DeathColor;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _Tiling;
   
            
            v2f vert (
                float4 vertex : POSITION, // vertex position input
                float2 uv : TEXCOORD0, // texture coordinate input
                out float4 outpos : SV_POSITION // clip space position output
                )
            {
                v2f o;
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                outpos = UnityObjectToClipPos(vertex);
                return o;
            }

            

            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                float2 screen = float2(screenPos.x / _ScreenParams.x, screenPos.y / _ScreenParams.y);
               
                //coords = TRANSFORM_TEX(coords, _MainTex);
            
                fixed4 col = tex2D(_MainTex, screen) * _Color;
                fixed4 actual = (1.0 - _Death)*col + (_Death)*_DeathColor;
            
                // apply fog

                return col;
            }
            ENDCG
        }
    }
}
