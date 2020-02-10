Shader "Custom/Unlit/Canvas2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Tex1 ("Texture 1", 2D) = "white" {}
        _Tex2 ("Texture 2", 2D) = "white" {}
        _Tex3 ("Texture 3", 2D) = "white" {}
        _Tex4 ("Texture 4", 2D) = "white" {}
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

            sampler2D _MainTex;
            sampler2D _Tex1, _Tex2, _Tex3, _Tex4;

            float _ActiveStates[4];

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                fixed4 mid = fixed4(.5, .5, .5, .5);

                fixed4 ct = fixed4(1.0, 1.0, 1.0, 1.0);

                fixed4 c1 = tex2D(_Tex1, uv);
                fixed4 c2 = tex2D(_Tex2, uv);
                fixed4 c3 = tex2D(_Tex3, uv);
                fixed4 c4 = tex2D(_Tex4, uv);
                
                if(_ActiveStates[0] > 0.0)
                    ct += (mid - c1);
                if(_ActiveStates[1] > 0.0)
                    ct += (mid - c2);
                if(_ActiveStates[2] > 0.0)
                    ct += (mid - c3);
                if(_ActiveStates[3] > 0.0)
                    ct += (mid - c4);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, ct);
                return ct;
            }
            ENDCG
        }
    }
}
