Shader "Custom/Instancing"
{
    
    //show values to edit in inspector
    Properties{
        
        _FriendSnaps ("FriendSnapshots", 2DArray) = ""{}
        [PerRendererData] _TextureIdx("Text Index", float) = 0.0
        [PerRendererData] _Color ("Color", Color) = (0, 0, 0, 1)
    }

    SubShader{
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" "ForceNoShadowCasting"="True"}
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass{
            CGPROGRAM
            //allow instancing
            #pragma multi_compile_instancing

            //shader functions
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0


            //use unity shader library
            #include "UnityCG.cginc"



            UNITY_DECLARE_TEX2DARRAY(_FriendSnaps);

            //per vertex data that comes from the model/parameters
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //per vertex data that gets passed from the vertex to the fragment function
            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos: TEXCOORD1;
                UNITY_FOG_COORDS(2)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _TextureIdx)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v){
                v2f o;
                
                //setup instance id
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.uv = v.uv;
                //v.vertex.y += _Time.y/10;
                //calculate the position in clip space to render the object
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET{

                //setup instance id
                UNITY_SETUP_INSTANCE_ID(i);

                //fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_FriendSnaps, float3(i.uv, UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIdx)));

                //col.a = lerp(0, 1, abs(_SinTime.w));
                col.a = saturate(1-i.worldPos.y/50 + .1);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
