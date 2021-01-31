// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerrainDeform"
{
	Properties
	{
		_radius("radius", Float) = 0
		_hardness("hardness", Float) = 0
		_Displacement("Displacement", Float) = 0
		_diffuse("diffuse", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 PlayerPos;
		uniform float _radius;
		uniform float _hardness;
		uniform float _Displacement;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform sampler2D _diffuse;
		uniform float4 _diffuse_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 appendResult7 = (float3(PlayerPos.x , PlayerPos.y , PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult7 ) / _radius );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult10 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness ) ) ) , 0.0 , 1.0 );
			v.vertex.xyz += ( ( ase_vertexNormal * ( 1.0 - clampResult10 ) * -10.0 ) * ( _Displacement * PlayerPos.w ) );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float3 tex2DNode27 = UnpackNormal( tex2D( _TextureSample0, uv_TextureSample0 ) );
			o.Normal = tex2DNode27;
			float2 uv_diffuse = i.uv_texcoord * _diffuse_ST.xy + _diffuse_ST.zw;
			o.Albedo = tex2D( _diffuse, uv_diffuse ).rgb;
			o.Occlusion = tex2DNode27.x;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
675;37;1117;964;2575.748;976.511;1.86814;True;False
Node;AmplifyShaderEditor.RangedFloatNode;3;-1573.766,-15.93292;Inherit;False;Property;_hardness;hardness;1;0;Create;True;0;0;0;False;0;False;0;0.57;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;5;-1600.3,119.6918;Inherit;False;Global;PlayerPos;PlayerPos;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;Infinity,Infinity,Infinity,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;7;-1253.016,149.615;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-1424.476,-147.4844;Inherit;False;Property;_radius;radius;0;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;4;-1409.696,47.62571;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;2;-992.8693,15.1073;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;13;-710.2711,-45.45049;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;10;-459.2786,-23.1229;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;19;-242.3445,-371.381;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;21;-196.7001,-127.9442;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;294.8963,128.5787;Inherit;False;Property;_Displacement;Displacement;2;0;Create;True;0;0;0;False;0;False;0;-0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-192.4895,-41.72521;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;474.3367,204.7763;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;41.30298,-192.0638;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;618.4638,158.9756;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;27;504.9347,-216.9552;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;26;302.7476,-365.2967;Inherit;True;Property;_diffuse;diffuse;3;0;Create;True;0;0;0;False;0;False;-1;1abcaf097598f6e4c85da1e34ee2c733;618c294964fd44e0095695acb197abad;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1036.688,-197.3177;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TerrainDeform;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;5;1
WireConnection;7;1;5;2
WireConnection;7;2;5;3
WireConnection;4;0;3;0
WireConnection;2;15;7;0
WireConnection;2;14;1;0
WireConnection;2;12;4;0
WireConnection;13;1;2;0
WireConnection;10;0;13;0
WireConnection;21;0;10;0
WireConnection;28;0;25;0
WireConnection;28;1;5;4
WireConnection;20;0;19;0
WireConnection;20;1;21;0
WireConnection;20;2;22;0
WireConnection;24;0;20;0
WireConnection;24;1;28;0
WireConnection;0;0;26;0
WireConnection;0;1;27;0
WireConnection;0;5;27;0
WireConnection;0;11;24;0
ASEEND*/
//CHKSM=B8FFFFE755C25A2A3CCC36E992C2414FCC1A435D