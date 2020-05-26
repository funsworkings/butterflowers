// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Butterfly"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Silhouette ("Silhouette", Color) = (0.5,0.5,0.5,0.2)
        _MainTex ("Main Texture", 2D) = "white" {}
        
        _Death ("Death", Range(0, 1)) = 0.0
        _DeathColor ("Death Color", Color) = (1,1,1,1)
        
        _Tiling ("Tiling", Float) = 1.0
    }
    SubShader
    {       
        Pass
        {
            ZTest Greater
            Blend DstColor OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
			{
				float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
				float3 normal : NORMAL;

				// Need this for basic functionality.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD01;
				float3 worldPos : TEXCOORD02;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            fixed4 _Silhouette;

            v2f vert ( appdata v )
            {
                UNITY_SETUP_INSTANCE_ID(v);

                v2f o;

                o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            } 

            fixed4 frag (v2f i ) : SV_Target
            {
                return _Silhouette;
            }

            ENDCG
      
		}
        Pass
        {
            Tags { "Queue"="Transparent" "RenderType"="Transparent" }
            LOD 100

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
			{
				float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
				float3 normal : NORMAL;

				// Need this for basic functionality.
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD01;
				float3 worldPos : TEXCOORD02;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
           
            //half _Death;
        
            fixed4 _Color;
            fixed4 _DeathColor;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _Tiling;

            // Per instance properties must be declared in this block.
			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Death)
            UNITY_INSTANCING_BUFFER_END(Props)

            
            v2f vert (appdata v)
            {
                v2f o;

                // Need this for basic functionality.
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }            

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float2 screenPos = i.vertex;
                float2 screen = float2(screenPos.x / _ScreenParams.x, screenPos.y / _ScreenParams.y);
               
                //coords = TRANSFORM_TEX(coords, _MainTex);

                float death = UNITY_ACCESS_INSTANCED_PROP(Props, _Death);
            
                fixed4 col = tex2D(_MainTex, screen) * _Color;
                fixed4 actual = (1.0 - death)*col + (death)*_DeathColor;
            
                // apply fog

                return col;
            }
            ENDCG
        }
    }
}
