// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NorthStar"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Texture1("Texture 1", 2D) = "white" {}
		_Size("Size", Range( 0 , 10)) = 1
		_TextureSample4("Texture Sample 4", 2D) = "white" {}
		_Float1("Float 1", Float) = 0
		_Texture3("Texture 3", 2D) = "white" {}
		_Texture0("Texture 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample4;
		uniform float _Float1;
		uniform sampler2D _Texture0;
		uniform float4 _Texture0_ST;
		uniform sampler2D _Texture1;
		uniform float _Size;
		uniform sampler2D _Texture3;
		uniform float4 _Texture3_ST;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float mulTime77 = _Time.y * 0.5;
			float2 appendResult79 = (float2(mulTime77 , 0.0));
			float2 uv_TexCoord80 = v.texcoord.xy + appendResult79;
			v.vertex.xyz += ( ( float4( ase_worldNormal , 0.0 ) * tex2Dlod( _TextureSample4, float4( uv_TexCoord80, 0, 0.0) ) ) * _Float1 ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Texture0 = i.uv_texcoord * _Texture0_ST.xy + _Texture0_ST.zw;
			float4 tex2DNode88 = tex2D( _Texture0, uv_Texture0 );
			o.Albedo = tex2DNode88.rgb;
			float mulTime3 = _Time.y * -0.1;
			float2 appendResult27 = (float2(0.0 , mulTime3));
			float2 temp_output_28_0 = (i.uv_texcoord*0.5 + appendResult27);
			float2 temp_output_4_0_g1 = (( temp_output_28_0 / _Size )).xy;
			float2 temp_cast_2 = (( mulTime3 * -0.1 )).xx;
			float2 temp_output_41_0_g1 = ( temp_cast_2 + 0.5 );
			float2 temp_output_17_0_g1 = float2( 0,-0.05 );
			float mulTime22_g1 = _Time.y * 0.1;
			float temp_output_27_0_g1 = frac( mulTime22_g1 );
			float2 temp_output_11_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * temp_output_27_0_g1 ) );
			float2 temp_output_12_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * frac( ( mulTime22_g1 + 0.5 ) ) ) );
			float3 lerpResult9_g1 = lerp( UnpackNormal( tex2D( _Texture1, temp_output_11_0_g1 ) ) , UnpackNormal( tex2D( _Texture1, temp_output_12_0_g1 ) ) , ( abs( ( temp_output_27_0_g1 - 0.5 ) ) / 0.5 ));
			float4 tex2DNode26 = tex2D( _Texture0, ( float3( temp_output_28_0 ,  0.0 ) + lerpResult9_g1 ).xy );
			float4 temp_output_62_0 = round( tex2DNode26 );
			o.Emission = temp_output_62_0.rgb;
			o.Alpha = 1;
			float2 uv_Texture3 = i.uv_texcoord * _Texture3_ST.xy + _Texture3_ST.zw;
			clip( tex2D( _Texture3, uv_Texture3 ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
675;37;1117;964;606.9644;862.0131;2.07101;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;3;-984.8737,-315.8326;Inherit;False;1;0;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-768.0916,-727.9777;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;27;-682.8407,-432.8266;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;77;-129.7008,965.4139;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;-478.5198,-551.5397;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;36;-489.1587,-314.2401;Inherit;True;Property;_Texture1;Texture 1;1;0;Create;True;0;0;0;False;0;False;None;72f2c5ed3cffb4860b8139ca5fe16a3e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-599.7276,-72.98247;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;317.2165,1005.668;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;35;-215.0586,-263.4025;Inherit;True;Flow;2;;1;acad10cc8145e1f4eb8042bebe2d9a42;2,50,1,51,1;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;0,-0.05;False;24;FLOAT;0.1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;525.2165,941.8843;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;32;94.67043,-222.1161;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;25;121.2902,-549.4739;Inherit;True;Property;_Texture0;Texture 0;10;0;Create;True;0;0;0;False;0;False;None;a2bb1f1e41ae1cd4fac910ebe93f00ae;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;69;769.2258,774.7552;Inherit;True;Property;_TextureSample4;Texture Sample 4;5;0;Create;True;0;0;0;False;0;False;-1;None;8e44fe896458128418bff33ed3981421;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;70;418.4594,553.8384;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;834.956,238.0737;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;26;391.2948,-214.2372;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;90;164.5352,110.2852;Inherit;True;Property;_Texture3;Texture 3;9;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;85;805.8857,517.8869;Inherit;False;Property;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;0;-0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;524.7943,49.55243;Inherit;False;Property;_Float2;Float 2;8;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;62;854.8875,-161.3634;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;88;399.3048,-513.8182;Inherit;True;Property;_TextureSample5;Texture Sample 5;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;67;406.6749,707.3006;Inherit;True;Property;_Texture2;Texture 2;6;0;Create;True;0;0;0;False;0;False;None;8e44fe896458128418bff33ed3981421;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;1107.016,474.8749;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;38;702.0676,-27.83242;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;91;405.8162,191.8116;Inherit;True;Property;_TextureSample6;Texture Sample 6;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;87;906.8453,-556.5449;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1137.225,-202.1385;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;NorthStar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;25;6.57;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;1;3;0
WireConnection;28;0;29;0
WireConnection;28;2;27;0
WireConnection;34;0;3;0
WireConnection;79;0;77;0
WireConnection;35;5;36;0
WireConnection;35;2;28;0
WireConnection;35;18;34;0
WireConnection;80;1;79;0
WireConnection;32;0;28;0
WireConnection;32;1;35;0
WireConnection;69;1;80;0
WireConnection;68;0;70;0
WireConnection;68;1;69;0
WireConnection;26;0;25;0
WireConnection;26;1;32;0
WireConnection;62;0;26;0
WireConnection;88;0;25;0
WireConnection;84;0;68;0
WireConnection;84;1;85;0
WireConnection;38;0;26;0
WireConnection;38;1;86;0
WireConnection;91;0;90;0
WireConnection;87;0;88;0
WireConnection;87;1;62;0
WireConnection;0;0;88;0
WireConnection;0;2;62;0
WireConnection;0;10;91;0
WireConnection;0;11;84;0
ASEEND*/
//CHKSM=B02E4049F4A9456FC8979EC097B99F768D48AC7B