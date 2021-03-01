// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NestedObject"
{
	Properties
	{
		_GridUnits("Grid Units", Float) = 1
		_InteractionRadius("Interaction Radius", Float) = 0
		_FillTex("FillTex", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_EmptyColor("Empty Color", Color) = (0,0,0,0)
		_hardness("hardness", Float) = 0
		_FullColor("Full Color", Color) = (0,0,0,0)
		_Displacement("Displacement", Float) = 1
		_PlayerPos("PlayerPos", Vector) = (0,0,0,0)
		_Emission("Emission", 2D) = "black" {}
		_Normal("Normal", 2D) = "white" {}
		_Smoothness("Smoothness", Float) = 0
		_RadialAmount("Radial Amount", Float) = 0
		_Opacity("Opacity", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _GridUnits;
		uniform float Fill;
		uniform float4 _PlayerPos;
		uniform float _InteractionRadius;
		uniform float _hardness;
		uniform float _Displacement;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _EmptyColor;
		uniform float4 _FullColor;
		uniform sampler2D _FillTex;
		uniform float _RadialAmount;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float _Smoothness;
		uniform float _Opacity;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float mulTime47 = _Time.y * ( Fill * 25.0 );
			float3 appendResult29 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult29 ) / _InteractionRadius );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult59 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness ) ) ) , 0.0 , 1.0 );
			float temp_output_52_0 = ( 1.0 - clampResult59 );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ase_worldPos - ( round( ( ase_worldPos * _GridUnits ) ) / _GridUnits ) ) + ( ( ( ( ( cos( mulTime47 ) + 1.0 ) / 2.0 ) * ( Fill * 0.005 ) ) + ( ( 3.0 * temp_output_52_0 * 10.0 ) * ( _Displacement * _PlayerPos.w ) ) ) * ase_vertexNormal ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = tex2D( _Normal, uv_Normal ).rgb;
			float4 lerpResult21 = lerp( _EmptyColor , _FullColor , Fill);
			float3 appendResult3 = (float3((float2( -1,-1 ) + (i.uv_texcoord - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) , 0.0));
			float3 normalizeResult12 = normalize( mul( float4( mul( float4( appendResult3 , 0.0 ), UNITY_MATRIX_V ).xyz , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 lerpResult18 = lerp( float3( i.uv_texcoord ,  0.0 ) , normalizeResult12 , _RadialAmount);
			o.Albedo = ( lerpResult21 * tex2D( _FillTex, lerpResult18.xy ) ).rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float4 tex2DNode28 = tex2D( _Emission, uv_Emission );
			o.Emission = tex2DNode28.rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			clip( ( tex2DNode28 + _Opacity ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
813;-541;924;364;698.6077;796.7086;1.756384;True;False
Node;AmplifyShaderEditor.RangedFloatNode;33;-2469.618,711.3388;Inherit;False;Property;_hardness;hardness;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;37;-2496.152,846.9643;Inherit;False;Property;_PlayerPos;PlayerPos;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;51;-2305.548,774.8983;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-605.9448,-1236.696;Inherit;False;Global;Fill;Fill;1;0;Create;True;0;0;0;False;0;False;0.51;0.6666667;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;29;-2148.868,876.8873;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2320.328,579.7874;Inherit;False;Property;_InteractionRadius;Interaction Radius;1;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;1;-1821.668,-1359.408;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1064.719,1050.57;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;25;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;45;-1888.718,742.3792;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;47;-839.9467,1147.113;Inherit;False;1;0;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;60;-1606.12,681.8214;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;2;-1627.338,-1339.961;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ViewMatrixNode;4;-1342.376,-1120.284;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.DynamicAppendNode;3;-1356.217,-1218.521;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CosOpNode;58;-603.1262,1144.997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;59;-1355.127,704.1487;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1088.338,685.5465;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-1184.223,-1072.274;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-485.3225,60.74261;Inherit;False;Property;_GridUnits;Grid Units;0;0;Create;True;0;0;0;False;0;False;1;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-956.5387,913.6623;Inherit;False;Property;_Displacement;Displacement;7;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-442.444,1205.966;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-572.4373,-91.03568;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;52;-1092.549,599.3276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;6;-1206.816,-846.3409;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-972.4098,-908.4725;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-279.145,1048.922;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.005;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;-290.0457,1207.801;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-224.9332,-49.54677;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;15;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-858.4149,587.4344;Inherit;False;3;3;0;FLOAT;3;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-770.1218,938.4313;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;14;-59.73834,-8.072479;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-788.1215,-728.8558;Inherit;False;Property;_RadialAmount;Radial Amount;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-88.07254,1193.112;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;12;-795.928,-834.9659;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-820.4243,-1018.948;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-607.9047,881.8633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-213.0069,-1088.099;Inherit;False;Property;_FullColor;Full Color;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;18;-555.5808,-878.9187;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;19;-218.3712,-1311.808;Inherit;False;Property;_EmptyColor;Empty Color;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;34;-529.1351,430.586;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;24;88.8322,91.79782;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;15;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;-219.7773,738.419;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;15;-576.3994,-1138.778;Inherit;True;Property;_FillTex;FillTex;2;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;22;-416.575,-447.0911;Inherit;True;Property;_Emission;Emission;9;0;Create;True;0;0;0;False;0;False;None;None;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleSubtractOpNode;31;352.9626,-93.47327;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;23;-402.0387,-662.5472;Inherit;True;Property;_Normal;Normal;10;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;28;-155.9374,-501.5121;Inherit;True;Property;_TextureSample1;Texture Sample 1;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;-112.6244,-863.3058;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;15207dc4ce6f9a34b97ef20fa6c6a3b8;15207dc4ce6f9a34b97ef20fa6c6a3b8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-38.07454,333.0631;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;26;411.5456,-208.0305;Inherit;False;Property;_Opacity;Opacity;14;0;Create;True;0;0;0;False;0;False;0;0.49;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;94.25991,-1004.613;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;62;583.1474,-256.5886;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;32;86.80573,-318.9679;Inherit;True;Property;_TextureSample2;Texture Sample 2;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;48;98.01318,-689.9245;Inherit;True;Property;_TextureSample3;Texture Sample 3;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;331.7895,-806.5636;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;443.4013,-397.3625;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;39;344.1829,-474.7023;Inherit;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;539.8085,-30.20888;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;50;-879.6196,-1148.038;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;41;-209.814,-327.7515;Inherit;True;Property;_OpacityMask;Opacity Mask;11;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;815.6007,-441.9418;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;NestedObject;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;3;False;-1;5;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;51;0;33;0
WireConnection;29;0;37;1
WireConnection;29;1;37;2
WireConnection;29;2;37;3
WireConnection;46;0;16;0
WireConnection;45;15;29;0
WireConnection;45;14;44;0
WireConnection;45;12;51;0
WireConnection;47;0;46;0
WireConnection;60;1;45;0
WireConnection;2;0;1;0
WireConnection;3;0;2;0
WireConnection;58;0;47;0
WireConnection;59;0;60;0
WireConnection;5;0;3;0
WireConnection;5;1;4;0
WireConnection;42;0;58;0
WireConnection;52;0;59;0
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;56;0;16;0
WireConnection;54;0;42;0
WireConnection;13;0;9;0
WireConnection;13;1;8;0
WireConnection;40;1;52;0
WireConnection;40;2;30;0
WireConnection;55;0;57;0
WireConnection;55;1;37;4
WireConnection;14;0;13;0
WireConnection;36;0;54;0
WireConnection;36;1;56;0
WireConnection;12;0;7;0
WireConnection;35;0;40;0
WireConnection;35;1;55;0
WireConnection;18;0;10;0
WireConnection;18;1;12;0
WireConnection;18;2;11;0
WireConnection;24;0;14;0
WireConnection;24;1;8;0
WireConnection;49;0;36;0
WireConnection;49;1;35;0
WireConnection;31;0;9;0
WireConnection;31;1;24;0
WireConnection;28;0;22;0
WireConnection;20;0;15;0
WireConnection;20;1;18;0
WireConnection;38;0;49;0
WireConnection;38;1;34;0
WireConnection;21;0;19;0
WireConnection;21;1;17;0
WireConnection;21;2;16;0
WireConnection;62;0;28;0
WireConnection;62;1;26;0
WireConnection;32;0;41;0
WireConnection;48;0;23;0
WireConnection;25;0;21;0
WireConnection;25;1;20;0
WireConnection;63;0;28;0
WireConnection;63;1;52;0
WireConnection;43;0;31;0
WireConnection;43;1;38;0
WireConnection;0;0;25;0
WireConnection;0;1;48;0
WireConnection;0;2;28;0
WireConnection;0;4;39;0
WireConnection;0;10;62;0
WireConnection;0;11;43;0
ASEEND*/
//CHKSM=12B24D1AF14E4D1162F11CC8ACF365AE1358E6B9