Shader /*ase_name*/ "WikiExamples/Simple Multi Pass Outline" /*end*/
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_MainBodyColor("Main Body Color", Color) = (0,0,0,1)
		/*ase_props*/
	}

		SubShader
	{
		Tags{ "Queue" = "Transparent" }

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always
			Blend Off

			Name "Outline"
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
		/*ase_pragma*/

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
				/*ase_vdata:p=p;n=n;uv0=tc0.xy*/
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
					/*ase_interp(0.zw,7):sp=sp.xyzw;uv0=tc0.xy*/
				};

				uniform float4 _OutlineColor;
				/*ase_globals*/

				v2f vert(appdata v /*ase_vert_input*/)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.texcoord.xy = v.texcoord.xy;
					o.texcoord.zw = 0;
					/*ase_vert_code:v=appdata;o=v2f*/
					v.vertex.xyz += /*ase_vert_out:Vertex Offset;Float3*/ float3(0, 0, 0) /*end*/;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}

				fixed4 frag(v2f i /*ase_frag_input*/) : SV_Target
				{
					fixed4 myColorVar;
				/*ase_frag_code:i=v2f*/
				myColorVar = /*ase_frag_out:Color;Float4*/_OutlineColor/*end*/;
				return myColorVar;
			}
			ENDCG
		}

		Pass
		{
				/*ase_main_pass*/
				Cull Back
				Blend Off

				Name "MainBody"
				CGPROGRAM
				#pragma target 3.0 
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				/*ase_pragma*/

				struct appdata
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
						/*ase_vdata:p=p;uv0=tc0.xy*/
					};

					struct v2f
					{
						float4 vertex : SV_POSITION;
						float4 texcoord : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
							/*ase_interp(0.zw,7):sp=sp.xyzw;uv0=tc0.xy*/
						};
						uniform float4 _MainBodyColor;
						/*ase_globals*/

						v2f vert(appdata v /*ase_vert_input*/)
						{
							v2f o;
							UNITY_SETUP_INSTANCE_ID(v);
							UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
							o.texcoord.xy = v.texcoord.xy;
							o.texcoord.zw = 0;
							/*ase_vert_code:v=appdata;o=v2f*/
							v.vertex.xyz += /*ase_vert_out:Vertex Offset;Float3*/ float3(0,0,0) /*end*/;
							o.vertex = UnityObjectToClipPos(v.vertex);
							return o;
						}

						fixed4 frag(v2f i /*ase_frag_input*/) : SV_Target
						{
							fixed4 myColorVar;
						/*ase_frag_code:i=v2f*/
						myColorVar = /*ase_frag_out:Color;Float4*/_MainBodyColor/*end*/;
						return myColorVar;
					}
					ENDCG
				}
				/*ase_pass_end*/
	}
		CustomEditor "ASEMaterialInspector"
}