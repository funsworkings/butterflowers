// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NorthStar"
{
	Properties
	{
		_Texture1("Texture 1", 2D) = "white" {}
		_Size("Size", Range( 0 , 10)) = 1
		_TextureSample4("Texture Sample 4", 2D) = "white" {}
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

		uniform sampler2D _TextureSample4;
		uniform float4 _TextureSample4_ST;
		uniform sampler2D _Texture0;
		uniform sampler2D _Texture1;
		uniform float _Size;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float2 uv_TextureSample4 = v.texcoord * _TextureSample4_ST.xy + _TextureSample4_ST.zw;
			v.vertex.xyz += ( float4( ase_worldNormal , 0.0 ) * tex2Dlod( _TextureSample4, float4( uv_TextureSample4, 0, 0.0) ) ).rgb;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime3 = _Time.y * -0.1;
			float2 appendResult27 = (float2(0.0 , mulTime3));
			float2 temp_output_28_0 = (i.uv_texcoord*1.0 + appendResult27);
			float2 temp_output_4_0_g1 = (( temp_output_28_0 / _Size )).xy;
			float temp_output_34_0 = ( mulTime3 * -0.1 );
			float2 temp_cast_1 = (temp_output_34_0).xx;
			float2 temp_output_41_0_g1 = ( temp_cast_1 + 0.5 );
			float2 temp_output_17_0_g1 = float2( 0,0.1 );
			float mulTime22_g1 = _Time.y * 0.03;
			float temp_output_27_0_g1 = frac( mulTime22_g1 );
			float2 temp_output_11_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * temp_output_27_0_g1 ) );
			float2 temp_output_12_0_g1 = ( temp_output_4_0_g1 + ( temp_output_41_0_g1 * temp_output_17_0_g1 * frac( ( mulTime22_g1 + 0.5 ) ) ) );
			float3 lerpResult9_g1 = lerp( UnpackNormal( tex2D( _Texture1, temp_output_11_0_g1 ) ) , UnpackNormal( tex2D( _Texture1, temp_output_12_0_g1 ) ) , ( abs( ( temp_output_27_0_g1 - 0.5 ) ) / 0.5 ));
			float4 tex2DNode26 = tex2D( _Texture0, ( float3( temp_output_28_0 ,  0.0 ) + lerpResult9_g1 ).xy );
			o.Albedo = tex2DNode26.rgb;
			o.Emission = round( pow( tex2DNode26 , 2.0 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
566;59;1538;964;437.2986;453.6082;1.578765;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;3;-984.8737,-315.8326;Inherit;False;1;0;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;-616.3906,-385.7581;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-768.0916,-727.9777;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;28;-478.5198,-551.5397;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;36;-531.5737,-269.478;Inherit;True;Property;_Texture1;Texture 1;0;0;Create;True;0;0;0;False;0;False;None;72f2c5ed3cffb4860b8139ca5fe16a3e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-754.7777,-192.0382;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;77;106.2992,1027.414;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-239.9772,-253.7119;Inherit;True;Flow;1;;1;acad10cc8145e1f4eb8042bebe2d9a42;2,50,1,51,1;5;5;SAMPLER2D;;False;2;FLOAT2;0,0;False;18;FLOAT2;0,0;False;17;FLOAT2;0,0.1;False;24;FLOAT;0.03;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;68.53938,-299.3214;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;317.2165,1005.668;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;25;58.15126,-492.1974;Inherit;True;Property;_Texture0;Texture 0;5;0;Create;True;0;0;0;False;0;False;None;a2bb1f1e41ae1cd4fac910ebe93f00ae;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;26;391.2948,-214.2372;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;525.2165,941.8843;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;67;406.6749,707.3006;Inherit;True;Property;_Texture2;Texture 2;4;0;Create;True;0;0;0;False;0;False;None;8e44fe896458128418bff33ed3981421;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;69;769.2258,774.7552;Inherit;True;Property;_TextureSample4;Texture Sample 4;4;0;Create;True;0;0;0;False;0;False;-1;None;8e44fe896458128418bff33ed3981421;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;70;418.4594,553.8384;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PowerNode;38;556.2271,65.53568;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;62;805.0564,-169.5801;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;33;-1035.84,-561.1832;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;822.5025,255.4173;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1137.225,-202.1385;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;NorthStar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;25;6.57;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;1;3;0
WireConnection;28;0;29;0
WireConnection;28;2;27;0
WireConnection;34;0;3;0
WireConnection;35;5;36;0
WireConnection;35;2;28;0
WireConnection;35;18;34;0
WireConnection;32;0;28;0
WireConnection;32;1;35;0
WireConnection;79;0;77;0
WireConnection;26;0;25;0
WireConnection;26;1;32;0
WireConnection;80;1;79;0
WireConnection;69;0;67;0
WireConnection;69;1;80;0
WireConnection;38;0;26;0
WireConnection;62;0;38;0
WireConnection;33;2;34;0
WireConnection;68;0;70;0
WireConnection;68;1;69;0
WireConnection;0;0;26;0
WireConnection;0;2;62;0
WireConnection;0;11;68;0
ASEEND*/
//CHKSM=60436E5B9417362C6A981BF590F886CE339382C6