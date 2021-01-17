// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NorthStar"
{
	Properties
	{
		_Texture1("Texture 1", 2D) = "white" {}
		_Size("Size", Range( 0 , 10)) = 1
		_Texture0("Texture 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Texture0;
		uniform sampler2D _Texture1;
		uniform float _Size;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float dotResult4_g2 = dot( ase_vertexNormal.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g2 = lerp( -1.0 , 1.0 , frac( ( sin( dotResult4_g2 ) * 43758.55 ) ));
			float temp_output_57_0 = lerpResult10_g2;
			float3 temp_cast_1 = (temp_output_57_0).xxx;
			v.vertex.xyz += temp_cast_1;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime3 = _Time.y * -0.25;
			float2 appendResult27 = (float2(0.0 , mulTime3));
			float2 temp_output_28_0 = (i.uv_texcoord*1.0 + appendResult27);
			float2 temp_output_4_0_g1 = (( temp_output_28_0 / _Size )).xy;
			float temp_output_34_0 = ( mulTime3 * -0.1 );
			float2 temp_cast_1 = (temp_output_34_0).xx;
			float2 temp_output_41_0_g1 = ( temp_cast_1 + 0.5 );
			float2 temp_output_17_0_g1 = float2( 0,0.04 );
			float mulTime22_g1 = _Time.y * 0.05;
			float temp_output_27_0_g1 = frac( mulTime22_g1 );
			float2 temp_output_11_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * temp_output_27_0_g1 ) );
			float2 temp_output_12_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * frac( ( mulTime22_g1 + 0.5 ) ) ) );
			float3 lerpResult9_g1 = lerp( UnpackNormal( tex2D( _Texture1, temp_output_11_0_g1 ) ) , UnpackNormal( tex2D( _Texture1, temp_output_12_0_g1 ) ) , ( abs( ( temp_output_27_0_g1 - 0.5 ) ) / 0.5 ));
			float4 tex2DNode26 = tex2D( _Texture0, ( float3( temp_output_28_0 ,  0.0 ) + lerpResult9_g1 ).xy );
			o.Albedo = tex2DNode26.rgb;
			o.Emission = pow( tex2DNode26 , 2.0 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
359;26;1538;964;1276.6;1080.115;2.498998;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;3;-1103.216,-304.4248;Inherit;False;1;0;FLOAT;-0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-887.8748,-716.5699;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;27;-736.1738,-374.3503;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;36;-614.2092,-244.2048;Inherit;True;Property;_Texture1;Texture 1;0;0;Create;True;0;0;0;False;0;False;None;72f2c5ed3cffb4860b8139ca5fe16a3e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;-398.6651,-490.6978;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-874.5609,-180.6303;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-357.8586,-244.2053;Inherit;False;Flow;1;;1;acad10cc8145e1f4eb8042bebe2d9a42;2,50,1,51,1;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;0,0.04;False;24;FLOAT;0.05;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;68.53938,-299.3214;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;25;-16,-496;Inherit;True;Property;_Texture0;Texture 0;4;0;Create;True;0;0;0;False;0;False;None;a2bb1f1e41ae1cd4fac910ebe93f00ae;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;26;330.4527,-242.757;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;60;416.0721,479.5226;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;10;-781.1345,-78.88387;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;47;410.1145,-690.1736;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;383.8362,98.25508;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;2,2,2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;39;-843.2461,188.3598;Inherit;False;1;0;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosTime;24;-959.8861,297.3469;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;33;-1155.623,-549.7754;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;653.9877,54.99426;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;50;343.9005,-970.8975;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;1,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;20;-514.2846,343.6085;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;43;-166.4977,526.9474;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;189.5386,50.17208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-242.5908,352.3867;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;58;-819.9305,68.97554;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FractNode;22;-748.4544,368.0854;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;56;850.6621,543.3112;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-313.545,464.2148;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;13;23.40237,334.6635;Inherit;False;FLOAT3;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;54;866.8777,-478.6887;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;31;-1027.792,-80.74879;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;15;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;55;-592.9623,98.93534;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;8;-422.5304,87.02905;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;52;779.3278,-796.8456;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;3;FLOAT3;-1,0,-1;False;4;FLOAT3;1,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;965.9265,-578.239;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FractNode;41;88.67521,237.6635;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;59;405.3948,295.3156;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScaleAndOffsetNode;46;30.75623,-787.2714;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;57;666.7646,293.7222;Inherit;False;Random Range;-1;;2;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;-1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;21;252.601,363.8656;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;649.2452,-810.2318;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;38;700.3588,-174.4774;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;48;-276.808,-599.8503;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;5;-185.5681,248.2106;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;15;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;42;110.586,150.516;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1045.856,-206.8862;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;NorthStar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;25;6.57;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;1;3;0
WireConnection;28;0;29;0
WireConnection;28;2;27;0
WireConnection;34;0;3;0
WireConnection;35;5;36;0
WireConnection;35;2;28;0
WireConnection;35;18;34;0
WireConnection;32;0;28;0
WireConnection;32;1;35;0
WireConnection;26;0;25;0
WireConnection;26;1;32;0
WireConnection;47;0;46;0
WireConnection;12;0;7;0
WireConnection;12;1;13;0
WireConnection;33;2;34;0
WireConnection;49;0;47;0
WireConnection;49;1;12;0
WireConnection;20;0;10;2
WireConnection;43;0;44;0
WireConnection;7;1;42;0
WireConnection;14;0;20;0
WireConnection;22;0;24;4
WireConnection;56;0;57;0
WireConnection;44;0;20;0
WireConnection;13;1;43;0
WireConnection;54;0;52;0
WireConnection;55;0;58;1
WireConnection;55;1;58;3
WireConnection;8;0;55;0
WireConnection;8;2;39;0
WireConnection;52;0;51;0
WireConnection;53;0;52;0
WireConnection;53;1;10;2
WireConnection;41;0;5;0
WireConnection;46;0;10;0
WireConnection;46;2;48;0
WireConnection;57;1;60;0
WireConnection;21;0;13;0
WireConnection;51;0;50;0
WireConnection;51;1;47;0
WireConnection;38;0;26;0
WireConnection;5;0;8;0
WireConnection;42;0;5;0
WireConnection;0;0;26;0
WireConnection;0;2;38;0
WireConnection;0;11;57;0
ASEEND*/
//CHKSM=F433569C8B0239953E0C65A2BEEAB30EEC65900D