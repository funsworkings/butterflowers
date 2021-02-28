// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WorldSnappingSil"
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_MainBodyColor("Main Body Color", Color) = (0,0,0,1)
		_LayerColor("Layer Color", Color) = (0,0,0,0)
		_GridUnits("Grid Units", Float) = 1
		_InteractionRadius("Interaction Radius", Float) = 0
		_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		_EmptyColor("Empty Color", Color) = (0,0,0,0)
		_hardness("hardness", Float) = 0
		_FullColor("Full Color", Color) = (1,1,1,0)
		_Displacement("Displacement", Float) = 1
		_PlayerPos("PlayerPos", Vector) = (0,0,0,0)
		_RadialAmount("Radial Amount", Float) = 1

	}

		SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" }

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
		#define ASE_NEEDS_VERT_NORMAL


		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
					
				};

				uniform float4 _OutlineColor;
				uniform float _GridUnits;
				uniform float4 _PlayerPos;
				uniform float _InteractionRadius;
				uniform float _hardness;
				uniform float _Displacement;
				uniform float4 _LayerColor;


				v2f vert(appdata v )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.texcoord.xy = v.texcoord.xy;
					o.texcoord.zw = 0;
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					float3 appendResult7 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
					float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult7 ) / _InteractionRadius );
					float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
					float clampResult10 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness ) ) ) , 0.0 , 1.0 );
					float temp_output_18_0 = ( ( 3.0 * ( 1.0 - clampResult10 ) * 10.0 ) * ( _Displacement * _PlayerPos.w ) );
					float3 temp_output_51_0 = ( ( ase_worldPos - ( round( ( ase_worldPos * _GridUnits ) ) / _GridUnits ) ) + ( temp_output_18_0 * v.normal ) );
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.texcoord.zw = 0;
					v.vertex.xyz += temp_output_51_0;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}

				fixed4 frag(v2f i ) : SV_Target
				{
					fixed4 myColorVar;
				
				myColorVar = _LayerColor;
				return myColorVar;
			}
			ENDCG
		}

		Pass
		{
				
				Cull Back
				Blend Off

				Name "MainBody"
				CGPROGRAM
				
				#pragma target 3.0 
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityShaderVariables.cginc"


				struct appdata
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
						float3 ase_normal : NORMAL;
					};

					struct v2f
					{
						float4 vertex : SV_POSITION;
						float4 texcoord : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
							
						};
						uniform float4 _MainBodyColor;
						uniform float _GridUnits;
						uniform float4 _PlayerPos;
						uniform float _InteractionRadius;
						uniform float _hardness;
						uniform float _Displacement;
						uniform float4 _EmptyColor;
						uniform float4 _FullColor;
						uniform float Fill;
						uniform sampler2D _AlbedoTexture;
						uniform float _RadialAmount;


						v2f vert(appdata v )
						{
							v2f o;
							UNITY_SETUP_INSTANCE_ID(v);
							UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
							o.texcoord.xy = v.texcoord.xy;
							o.texcoord.zw = 0;
							float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
							float3 appendResult7 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
							float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult7 ) / _InteractionRadius );
							float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
							float clampResult10 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness ) ) ) , 0.0 , 1.0 );
							float temp_output_18_0 = ( ( 3.0 * ( 1.0 - clampResult10 ) * 10.0 ) * ( _Displacement * _PlayerPos.w ) );
							float3 temp_output_51_0 = ( ( ase_worldPos - ( round( ( ase_worldPos * _GridUnits ) ) / _GridUnits ) ) + ( temp_output_18_0 * v.ase_normal ) );
							
							
							//setting value to unused interpolator channels and avoid initialization warnings
							o.texcoord.zw = 0;
							v.vertex.xyz += temp_output_51_0;
							o.vertex = UnityObjectToClipPos(v.vertex);
							return o;
						}

						fixed4 frag(v2f i ) : SV_Target
						{
							fixed4 myColorVar;
						float4 lerpResult41 = lerp( _EmptyColor , _FullColor , Fill);
						float2 texCoord35 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
						float3 appendResult31 = (float3((float2( -1,-1 ) + (i.texcoord.xy - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) , 0.0));
						float3 normalizeResult46 = normalize( mul( float4( mul( float4( appendResult31 , 0.0 ), UNITY_MATRIX_V ).xyz , 0.0 ), unity_ObjectToWorld ).xyz );
						float3 lerpResult40 = lerp( float3( texCoord35 ,  0.0 ) , normalizeResult46 , _RadialAmount);
						
						myColorVar = ( lerpResult41 * tex2D( _AlbedoTexture, lerpResult40.xy ) );
						return myColorVar;
					}
					ENDCG
				}
				
	}
		CustomEditor "ASEMaterialInspector"
	
	
}/*ASEBEGIN
Version=18800
307.3333;18;1306;881;2322.979;-517.4076;1.928682;True;False
Node;AmplifyShaderEditor.RangedFloatNode;3;-2932.435,1081.971;Inherit;False;Property;_hardness;hardness;5;0;Create;True;0;0;0;False;0;False;0;0.57;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;4;-2958.969,1217.597;Inherit;False;Property;_PlayerPos;PlayerPos;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;7;-2611.685,1247.52;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2783.145,950.4197;Inherit;False;Property;_InteractionRadius;Interaction Radius;2;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;5;-2768.365,1145.531;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;28;-2755.21,-645.2505;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;29;-2560.88,-625.8035;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;8;-2351.535,1113.012;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;9;-2068.936,1052.454;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewMatrixNode;30;-2275.918,-406.1274;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-2289.759,-504.3644;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;21;-1194.139,170.1546;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-2117.765,-358.1167;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1168.634,364.1906;Inherit;False;Property;_GridUnits;Grid Units;1;0;Create;True;0;0;0;False;0;False;1;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;10;-1817.943,1074.781;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;32;-2140.358,-132.1837;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-983.2946,261.0017;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;15;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-1905.952,-194.3153;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;12;-1555.364,969.96;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1551.153,1056.179;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1419.354,1284.295;Inherit;False;Property;_Displacement;Displacement;7;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;22;-833.0864,330.6548;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1721.664,-14.69866;Inherit;False;Property;_RadialAmount;Radial Amount;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-1753.967,-304.7914;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1232.937,1309.064;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1317.362,905.8405;Inherit;False;3;3;0;FLOAT;3;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;46;-1729.47,-120.8087;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;37;-1509.942,-424.6211;Inherit;True;Property;_AlbedoTexture;Albedo Texture;3;0;Create;True;0;0;0;False;0;False;15207dc4ce6f9a34b97ef20fa6c6a3b8;15207dc4ce6f9a34b97ef20fa6c6a3b8;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1070.72,1252.496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1539.487,-522.5386;Inherit;False;Global;Fill;Fill;1;0;Create;True;0;0;0;False;0;False;0;0.3333333;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;54;-1036.548,660.1301;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;23;-691.0001,343.4812;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;15;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;38;-1151.913,-597.6506;Inherit;False;Property;_EmptyColor;Empty Color;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;36;-1146.549,-373.9421;Inherit;False;Property;_FullColor;Full Color;6;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;40;-1489.123,-164.7616;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;41;-839.2823,-290.4557;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;26;-519.7039,164.6531;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;42;-1046.167,-149.1487;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;15207dc4ce6f9a34b97ef20fa6c6a3b8;15207dc4ce6f9a34b97ef20fa6c6a3b8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-683.3384,667.6401;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;50;-1039.628,824.9386;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexCoordVertexDataNode;43;-1813.162,-433.8813;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-601.7528,-92.40646;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-340.3186,231.7543;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CosOpNode;17;-1205.942,1505.63;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;53;-951.5226,1584.07;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;52;-1091.523,1517.07;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;16;-1389.762,1493.746;Inherit;False;1;0;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-911.1336,1293.521;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-409.2846,-235.2271;Inherit;False;Property;_LayerColor;Layer Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,-130.8281;Float;False;False;-1;2;ASEMaterialInspector;0;10;New Amplify Shader;a08ae67085c7dc544bd3a62d88079edc;True;Outline;0;0;Outline;2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;Queue=Transparent=Queue=0;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;True;2;False;-1;True;7;False;-1;False;False;True;2;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;4.768372E-07,-6.828089;Float;False;True;-1;2;ASEMaterialInspector;0;10;WorldSnappingSil;a08ae67085c7dc544bd3a62d88079edc;True;MainBody;0;1;MainBody;2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;Queue=Transparent=Queue=0;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;2;True;True;False;;False;0
WireConnection;7;0;4;1
WireConnection;7;1;4;2
WireConnection;7;2;4;3
WireConnection;5;0;3;0
WireConnection;29;0;28;0
WireConnection;8;15;7;0
WireConnection;8;14;6;0
WireConnection;8;12;5;0
WireConnection;9;1;8;0
WireConnection;31;0;29;0
WireConnection;33;0;31;0
WireConnection;33;1;30;0
WireConnection;10;0;9;0
WireConnection;27;0;21;0
WireConnection;27;1;19;0
WireConnection;34;0;33;0
WireConnection;34;1;32;0
WireConnection;12;0;10;0
WireConnection;22;0;27;0
WireConnection;14;0;11;0
WireConnection;14;1;4;4
WireConnection;15;1;12;0
WireConnection;15;2;13;0
WireConnection;46;0;34;0
WireConnection;18;0;15;0
WireConnection;18;1;14;0
WireConnection;23;0;22;0
WireConnection;23;1;19;0
WireConnection;40;0;35;0
WireConnection;40;1;46;0
WireConnection;40;2;45;0
WireConnection;41;0;38;0
WireConnection;41;1;36;0
WireConnection;41;2;39;0
WireConnection;26;0;21;0
WireConnection;26;1;23;0
WireConnection;42;0;37;0
WireConnection;42;1;40;0
WireConnection;49;0;18;0
WireConnection;49;1;54;0
WireConnection;44;0;41;0
WireConnection;44;1;42;0
WireConnection;51;0;26;0
WireConnection;51;1;49;0
WireConnection;17;0;16;0
WireConnection;53;0;52;0
WireConnection;52;0;17;0
WireConnection;24;0;18;0
WireConnection;24;1;53;0
WireConnection;0;0;2;0
WireConnection;0;1;51;0
WireConnection;1;0;44;0
WireConnection;1;1;51;0
ASEEND*/
//CHKSM=D77263910D78340ACDBB7FB111FA52B43B9DF482