// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NorthStar"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = -0.04
		_InteractionRadius("Interaction Radius", Float) = 0
		_Size("Size", Range( 0 , 10)) = 1
		_Flow("Flow", 2D) = "white" {}
		_Hardness("Hardness", Float) = 0
		_Displacement("Displacement", Float) = 0
		_PlayerPos("PlayerPos", Vector) = (0,0,0,0)
		_VertexOffsetMask("Vertex Offset Mask", 2D) = "white" {}
		_VertexOffsetStrength("Vertex Offset Strength", Float) = 0
		_RoundedEmission("Rounded Emission", 2D) = "white" {}
		_OpacityMask("Opacity Mask", 2D) = "white" {}
		_MaterialTexture("Material Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Float) = 0
		_Normal("Normal", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform sampler2D _VertexOffsetMask;
		uniform float _VertexOffsetStrength;
		uniform float4 _PlayerPos;
		uniform float _InteractionRadius;
		uniform float _Hardness;
		uniform float _Displacement;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _MaterialTexture;
		uniform float4 _MaterialTexture_ST;
		uniform sampler2D _RoundedEmission;
		uniform sampler2D _Flow;
		uniform float _Size;
		uniform float _Smoothness;
		uniform sampler2D _OpacityMask;
		uniform float4 _OpacityMask_ST;
		uniform float _Cutoff = -0.04;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float mulTime77 = _Time.y * 0.5;
			float2 appendResult79 = (float2(mulTime77 , 0.0));
			float2 uv_TexCoord80 = v.texcoord.xy + appendResult79;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 appendResult94 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult94 ) / _InteractionRadius );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult99 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _Hardness ) ) ) , 0.0 , 1.0 );
			v.vertex.xyz += ( ( ( float4( ase_worldNormal , 0.0 ) * tex2Dlod( _VertexOffsetMask, float4( uv_TexCoord80, 0, 0.0) ) ) * _VertexOffsetStrength ) + float4( ( ( ase_vertexNormal * ( 1.0 - clampResult99 ) * -2.0 ) * ( _Displacement * _PlayerPos.w ) ) , 0.0 ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = tex2D( _Normal, uv_Normal ).rgb;
			float2 uv_MaterialTexture = i.uv_texcoord * _MaterialTexture_ST.xy + _MaterialTexture_ST.zw;
			o.Albedo = tex2D( _MaterialTexture, uv_MaterialTexture ).rgb;
			float mulTime3 = _Time.y * -0.1;
			float2 appendResult27 = (float2(0.0 , mulTime3));
			float2 temp_output_28_0 = (i.uv_texcoord*0.5 + appendResult27);
			float2 temp_output_4_0_g2 = (( temp_output_28_0 / _Size )).xy;
			float2 temp_cast_3 = (( mulTime3 * -0.1 )).xx;
			float2 temp_output_41_0_g2 = ( temp_cast_3 + 0.5 );
			float2 temp_output_17_0_g2 = float2( 0,-0.05 );
			float mulTime22_g2 = _Time.y * 0.1;
			float temp_output_27_0_g2 = frac( mulTime22_g2 );
			float2 temp_output_11_0_g2 = ( temp_output_4_0_g2 + ( temp_output_41_0_g2 * temp_output_17_0_g2 * temp_output_27_0_g2 ) );
			float2 temp_output_12_0_g2 = ( temp_output_4_0_g2 + ( temp_output_41_0_g2 * temp_output_17_0_g2 * frac( ( mulTime22_g2 + 0.5 ) ) ) );
			float3 lerpResult9_g2 = lerp( UnpackNormal( tex2D( _Flow, temp_output_11_0_g2 ) ) , UnpackNormal( tex2D( _Flow, temp_output_12_0_g2 ) ) , ( abs( ( temp_output_27_0_g2 - 0.5 ) ) / 0.5 ));
			o.Emission = round( tex2D( _RoundedEmission, ( float3( temp_output_28_0 ,  0.0 ) + lerpResult9_g2 ).xy ) ).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			float2 uv_OpacityMask = i.uv_texcoord * _OpacityMask_ST.xy + _OpacityMask_ST.zw;
			clip( tex2D( _OpacityMask, uv_OpacityMask ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
675;37;1117;964;251.9923;-313.4506;1.455385;True;False
Node;AmplifyShaderEditor.Vector4Node;93;-1507.141,1492.919;Inherit;False;Property;_PlayerPos;PlayerPos;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;92;-1480.607,1357.294;Inherit;False;Property;_Hardness;Hardness;3;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;96;-1316.537,1420.853;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;94;-1159.857,1522.842;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-1331.317,1225.743;Inherit;False;Property;_InteractionRadius;Interaction Radius;1;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;97;-899.7102,1388.334;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;77;-1053.965,912.7399;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;3;-980.097,-333.3471;Inherit;False;1;0;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-893.0545,-656.6911;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;27;-678.064,-440.7877;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DistanceOpNode;98;-617.112,1327.777;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;-835.063,919.0997;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;-478.5198,-543.5786;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;99;-366.1195,1350.104;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;36;-489.97,-249.7948;Inherit;True;Property;_Flow;Flow;2;0;Create;True;0;0;0;False;0;False;None;72f2c5ed3cffb4860b8139ca5fe16a3e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-611.1251,-65.61578;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;-630.1443,875.3444;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;100;-54.72077,1477.57;Inherit;False;Constant;_VertexNormalStrength;Vertex Normal Strength;5;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;101;-58.93136,1366.257;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-159.3747,-108.2523;Inherit;True;Flow;4;;2;acad10cc8145e1f4eb8042bebe2d9a42;2,50,1,51,1;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;0,-0.05;False;24;FLOAT;0.1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;69;-386.1349,708.2152;Inherit;True;Property;_VertexOffsetMask;Vertex Offset Mask;6;0;Create;True;0;0;0;False;0;False;-1;None;8e44fe896458128418bff33ed3981421;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;70;-236.0413,496.6024;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;113;97.85165,1699.603;Inherit;False;Property;_Displacement;Displacement;4;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;102;-115.5544,1144.777;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;32;171.4522,-142.3232;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-11.05307,681.6741;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;272.7896,1756.183;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;168.093,1324.094;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;85;208.3567,963.1857;Inherit;False;Property;_VertexOffsetStrength;Vertex Offset Strength;7;0;Create;True;0;0;0;False;0;False;0;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;118;363.8857,-313.5277;Inherit;True;Property;_RoundedEmission;Rounded Emission;8;0;Create;True;0;0;0;False;0;False;None;5345ffbc111e0463e9be399a81e48ce7;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;26;641.9263,-166.4978;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;415.3757,1619.469;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;90;167.2417,179.2999;Inherit;True;Property;_OpacityMask;Opacity Mask;9;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;120;644.6359,-406.1251;Inherit;True;Property;_Normal;Normal;12;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;25;610.246,-796.5487;Inherit;True;Property;_MaterialTexture;Material Texture;10;0;Create;True;0;0;0;False;0;False;None;5345ffbc111e0463e9be399a81e48ce7;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;485.9756,874.1152;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;117;824.4017,829.0492;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;91;405.8162,191.8116;Inherit;True;Property;_TextureSample6;Texture Sample 6;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;62;1061.705,-183.5461;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;88;844.4398,-795.317;Inherit;True;Property;_TextureSample5;Texture Sample 5;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;122;932.3702,-415.1168;Inherit;True;Property;_TextureSample4;Texture Sample 4;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;119;729.9822,63.91272;Inherit;False;Property;_Smoothness;Smoothness;11;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1406.587,-253.5539;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;NorthStar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;-0.04;True;True;0;True;Opaque;;AlphaTest;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;25;6.57;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;96;0;92;0
WireConnection;94;0;93;1
WireConnection;94;1;93;2
WireConnection;94;2;93;3
WireConnection;97;15;94;0
WireConnection;97;14;95;0
WireConnection;97;12;96;0
WireConnection;27;1;3;0
WireConnection;98;1;97;0
WireConnection;79;0;77;0
WireConnection;28;0;29;0
WireConnection;28;2;27;0
WireConnection;99;0;98;0
WireConnection;34;0;3;0
WireConnection;80;1;79;0
WireConnection;101;0;99;0
WireConnection;35;5;36;0
WireConnection;35;2;28;0
WireConnection;35;18;34;0
WireConnection;69;1;80;0
WireConnection;32;0;28;0
WireConnection;32;1;35;0
WireConnection;68;0;70;0
WireConnection;68;1;69;0
WireConnection;114;0;113;0
WireConnection;114;1;93;4
WireConnection;103;0;102;0
WireConnection;103;1;101;0
WireConnection;103;2;100;0
WireConnection;26;0;118;0
WireConnection;26;1;32;0
WireConnection;115;0;103;0
WireConnection;115;1;114;0
WireConnection;84;0;68;0
WireConnection;84;1;85;0
WireConnection;117;0;84;0
WireConnection;117;1;115;0
WireConnection;91;0;90;0
WireConnection;62;0;26;0
WireConnection;88;0;25;0
WireConnection;122;0;120;0
WireConnection;0;0;88;0
WireConnection;0;1;122;0
WireConnection;0;2;62;0
WireConnection;0;4;119;0
WireConnection;0;10;91;0
WireConnection;0;11;117;0
ASEEND*/
//CHKSM=E5D2C75C9C765BEC5D7E10973C57F1B41AA6F62A