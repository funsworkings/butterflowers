// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NestVertexSnapping"
{
	Properties
	{
		_Float0("Float 0", Float) = 1
		_radius1("radius", Float) = 0
		_Texture0("Texture 0", 2D) = "white" {}
		_Color0("Color 0", Color) = (0,0,0,0)
		_hardness1("hardness", Float) = 0
		_Color1("Color 1", Color) = (0,0,0,0)
		_Displacement1("Displacement", Float) = 0
		_PlayerPos("PlayerPos", Vector) = (0,0,0,0)
		_Texture1("Texture 1", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
		};

		uniform float _Float0;
		uniform float4 _PlayerPos;
		uniform float _radius1;
		uniform float _hardness1;
		uniform float _Displacement1;
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float Fill;
		uniform sampler2D _Texture0;
		uniform sampler2D _Texture1;
		uniform float4 _Texture1_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 appendResult43 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult43 ) / _radius1 );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult48 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness1 ) ) ) , 0.0 , 1.0 );
			float3 temp_output_54_0 = ( ase_vertexNormal * ( 1.0 - clampResult48 ) * -10.0 );
			float3 temp_output_55_0 = ( temp_output_54_0 * ( _Displacement1 * _PlayerPos.w ) );
			float mulTime72 = _Time.y * -5.0;
			float3 temp_output_69_0 = ( _Float0 + ( temp_output_55_0 * cos( mulTime72 ) ) );
			float3 temp_output_9_0 = ( ( round( ( ase_worldPos * temp_output_69_0 ) ) / temp_output_69_0 ) - ase_worldPos );
			v.vertex.xyz += temp_output_9_0;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult39 = lerp( _Color0 , _Color1 , Fill);
			float3 appendResult28 = (float3((float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) , 0.0));
			float3 normalizeResult26 = normalize( mul( float4( mul( float4( appendResult28 , 0.0 ), UNITY_MATRIX_V ).xyz , 0.0 ), unity_ObjectToWorld ).xyz );
			float4 temp_output_40_0 = ( lerpResult39 * tex2D( _Texture0, normalizeResult26.xy ) );
			o.Albedo = temp_output_40_0.rgb;
			float2 uv_Texture1 = i.uv_texcoord * _Texture1_ST.xy + _Texture1_ST.zw;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float3 ase_worldPos = i.worldPos;
			float3 appendResult43 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult43 ) / _radius1 );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult48 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness1 ) ) ) , 0.0 , 1.0 );
			float3 temp_output_54_0 = ( ase_vertexNormal * ( 1.0 - clampResult48 ) * -10.0 );
			float3 temp_output_55_0 = ( temp_output_54_0 * ( _Displacement1 * _PlayerPos.w ) );
			float4 temp_output_78_0 = ( tex2D( _Texture1, uv_Texture1 ) * float4( temp_output_55_0 , 0.0 ) );
			o.Emission = temp_output_78_0.rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
675;37;1117;964;2045.409;217.8914;2.192016;True;False
Node;AmplifyShaderEditor.RangedFloatNode;41;-2560.534,946.7665;Inherit;False;Property;_hardness1;hardness;5;0;Create;True;0;0;0;False;0;False;0;0.57;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;42;-2587.068,1082.392;Inherit;False;Property;_PlayerPos;PlayerPos;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;43;-2239.784,1112.315;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2411.244,815.2151;Inherit;False;Property;_radius1;radius;1;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;45;-2396.464,1010.326;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;46;-1979.634,977.8069;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;47;-1697.036,917.249;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;48;-1446.043,939.5764;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;49;-1183.465,834.7553;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;51;-1229.109,591.3188;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;52;-1179.254,920.9742;Inherit;False;Constant;_Float2;Float 1;5;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-1023.064,1522.05;Inherit;False;Property;_Displacement1;Displacement;8;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-808.6758,1254.743;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-945.4625,770.6358;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;72;-710.1241,1349.959;Inherit;False;1;0;FLOAT;-5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;22;-1143.172,-810.1746;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosOpNode;74;-516.9673,1462.991;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-723.8638,1123.552;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;27;-905.9423,-764.9879;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-379.9227,1231.193;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewMatrixNode;21;-1049.975,-440.2086;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.DynamicAppendNode;28;-634.8224,-643.5488;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-632.6004,189.666;Inherit;False;Property;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;1;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-891.8215,-392.198;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;25;-914.4147,-166.265;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-429.7069,177.862;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1;-667.8035,1.995441;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-287.383,-47.13243;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;15,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-680.009,-228.3966;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;26;-550.0975,-138.0232;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;38;-310.3827,-897.888;Inherit;False;Property;_Color1;Color 1;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;36;-658.4531,-866.1272;Inherit;True;Property;_Texture0;Texture 0;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;35;-691.7468,-1011.497;Inherit;False;Global;Fill;Fill;1;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;37;-309.2874,-1076.38;Inherit;False;Property;_Color0;Color 0;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;3;-143.1045,69.95857;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;76;-377.3409,-323.9758;Inherit;True;Property;_Texture1;Texture 1;10;0;Create;True;0;0;0;False;0;False;None;2c29c08dd1c0b6749b7cd0fcff7a29fd;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;77;-143.7409,-305.2044;Inherit;True;Property;_TextureSample1;Texture Sample 1;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;16;-403.1717,-536.5652;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;15207dc4ce6f9a34b97ef20fa6c6a3b8;15207dc4ce6f9a34b97ef20fa6c6a3b8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;39;3.34365,-769.1851;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;4;-6.533952,184.829;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;15,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinTimeNode;29;-380.7804,557.3472;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;62;-1477.071,1147.289;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-890.2546,1120.474;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;68;-527.9182,1085.517;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;217.006,-537.0402;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;30;-73.69568,531.9437;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;65;-1104.53,1031.068;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;58;-1693.677,1147.289;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;158.688,-215.5186;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;306.7737,-319.8044;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;31;-213.8769,1022.058;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FractNode;73;-519.4355,1349.901;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-2343.991,1277.124;Inherit;False;Property;_radius3;radius2;3;0;Create;True;0;0;0;False;0;False;0;3.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;57;-781.0373,353.359;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;61;-1978.156,1138.921;Inherit;False;SphereMask;-1;;2;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosTime;33;-290.5137,723.4187;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-2233.134,1345.117;Inherit;False;Property;_hardness3;hardness2;7;0;Create;True;0;0;0;False;0;False;0;2.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;135.1416,399.8177;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;295.4663,463.9336;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-494.395,684.9237;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;59;-1191.655,1246.769;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-1155.395,1343.479;Inherit;False;Constant;_Float1;Float 0;5;0;Create;True;0;0;0;False;0;False;1.33;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;510.145,-83.15596;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NestVertexSnapping;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;43;0;42;1
WireConnection;43;1;42;2
WireConnection;43;2;42;3
WireConnection;45;0;41;0
WireConnection;46;15;43;0
WireConnection;46;14;44;0
WireConnection;46;12;45;0
WireConnection;47;1;46;0
WireConnection;48;0;47;0
WireConnection;49;0;48;0
WireConnection;53;0;50;0
WireConnection;53;1;42;4
WireConnection;54;0;51;0
WireConnection;54;1;49;0
WireConnection;54;2;52;0
WireConnection;74;0;72;0
WireConnection;55;0;54;0
WireConnection;55;1;53;0
WireConnection;27;0;22;0
WireConnection;71;0;55;0
WireConnection;71;1;74;0
WireConnection;28;0;27;0
WireConnection;23;0;28;0
WireConnection;23;1;21;0
WireConnection;69;0;5;0
WireConnection;69;1;71;0
WireConnection;2;0;1;0
WireConnection;2;1;69;0
WireConnection;24;0;23;0
WireConnection;24;1;25;0
WireConnection;26;0;24;0
WireConnection;3;0;2;0
WireConnection;77;0;76;0
WireConnection;16;0;36;0
WireConnection;16;1;26;0
WireConnection;39;0;37;0
WireConnection;39;1;38;0
WireConnection;39;2;35;0
WireConnection;4;0;3;0
WireConnection;4;1;69;0
WireConnection;62;0;58;0
WireConnection;63;0;65;0
WireConnection;63;1;59;0
WireConnection;63;2;64;0
WireConnection;68;0;55;0
WireConnection;40;0;39;0
WireConnection;40;1;16;0
WireConnection;30;0;31;2
WireConnection;30;2;34;0
WireConnection;58;0;61;0
WireConnection;78;0;77;0
WireConnection;78;1;55;0
WireConnection;75;0;40;0
WireConnection;75;1;78;0
WireConnection;73;0;72;0
WireConnection;57;0;54;0
WireConnection;57;1;63;0
WireConnection;57;2;48;0
WireConnection;61;15;43;0
WireConnection;61;14;60;0
WireConnection;61;12;56;0
WireConnection;9;0;4;0
WireConnection;9;1;1;0
WireConnection;32;0;9;0
WireConnection;32;1;30;0
WireConnection;59;0;62;0
WireConnection;0;0;40;0
WireConnection;0;2;78;0
WireConnection;0;11;9;0
ASEEND*/
//CHKSM=4D42DCED0695F72DAE7202076EE59EA6D104FABC