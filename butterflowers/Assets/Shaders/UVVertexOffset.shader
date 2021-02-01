// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UVVertexOffset"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_radius("radius", Float) = 0
		_treetop_1("treetop_1", 2D) = "white" {}
		_Color0("Color 0", Color) = (1,1,1,1)
		_Size("Size", Float) = 1
		_hardness("hardness", Float) = 0
		_Float0("Float 0", Float) = 0
		_BaseFlip("Base Flip", Float) = 0
		_Displacement("Displacement", Float) = 0
		_PlayerPos("PlayerPos", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" }
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

		uniform float _Float0;
		uniform float4 _PlayerPos;
		uniform float _radius;
		uniform float _hardness;
		uniform float _Displacement;
		uniform float _Size;
		uniform float _BaseFlip;
		uniform float4 _Color0;
		uniform sampler2D _treetop_1;
		uniform float4 _treetop_1_ST;
		uniform float _Cutoff = 0.5;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float simplePerlin3D31 = snoise( ase_worldPos*5.0 );
			simplePerlin3D31 = simplePerlin3D31*0.5 + 0.5;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float mulTime45 = _Time.y * 0.001;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 appendResult50 = (float3(_PlayerPos.x , _PlayerPos.y , _PlayerPos.z));
			float3 temp_output_5_0_g1 = ( ( ase_worldPos - appendResult50 ) / _radius );
			float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
			float clampResult53 = clamp( distance( 0.0 , pow( saturate( dotResult8_g1 ) , ( 1.0 - _hardness ) ) ) , 0.0 , 1.0 );
			float3 temp_output_60_0 = ( ( ase_vertexNormal * ( 1.0 - clampResult53 ) * -10.0 ) * ( _Displacement * _PlayerPos.w ) );
			float simplePerlin3D28 = snoise( (ase_vertex3Pos*1.0 + ( mulTime45 + temp_output_60_0 ))*5.0 );
			simplePerlin3D28 = simplePerlin3D28*0.5 + 0.5;
			float cos23 = cos( ( simplePerlin3D28 * 15.0 ) );
			float sin23 = sin( ( simplePerlin3D28 * 15.0 ) );
			float2 rotator23 = mul( v.texcoord.xy - float2( 0.5,0.5 ) , float2x2( cos23 , -sin23 , sin23 , cos23 )) + float2( 0.5,0.5 );
			float3 appendResult6 = (float3((float2( -1,-1 ) + (rotator23 - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) , 0.0));
			float3 normalizeResult9 = normalize( mul( float4( mul( float4( appendResult6 , 0.0 ), UNITY_MATRIX_V ).xyz , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 lerpResult41 = lerp( float3(0,0,0) , ( ( (-1.0 + (simplePerlin3D31 - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * _Float0 ) + ( normalizeResult9 * _Size ) ) , ( _BaseFlip + temp_output_60_0 ));
			v.vertex.xyz += lerpResult41;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_treetop_1 = i.uv_texcoord * _treetop_1_ST.xy + _treetop_1_ST.zw;
			float4 tex2DNode10 = tex2D( _treetop_1, uv_treetop_1 );
			o.Albedo = ( _Color0 * tex2DNode10 ).rgb;
			o.Alpha = 1;
			clip( tex2DNode10.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
675;37;1117;964;1140.196;474.9871;1.111256;True;False
Node;AmplifyShaderEditor.RangedFloatNode;46;-2489.723,566.9594;Inherit;False;Property;_hardness;hardness;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;47;-2516.257,702.584;Inherit;False;Property;_PlayerPos;PlayerPos;11;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;50;-2168.973,732.5073;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2340.433,435.4076;Inherit;False;Property;_radius;radius;1;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;49;-2325.653,630.518;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;51;-1908.827,597.9996;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;52;-1626.229,537.4417;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;53;-1375.236,559.7693;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1108.447,541.1669;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;-10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-948.9114,860.5126;Inherit;False;Property;_Displacement;Displacement;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;55;-1158.302,211.511;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;54;-1112.658,454.9478;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-874.6542,390.8282;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-737.8676,874.9351;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;45;-2201.425,1746.378;Inherit;False;1;0;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-526.2822,846.8021;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-1982.996,1748.88;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;27;-2065.046,1573.555;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;44;-1835.487,1677.502;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;28;-1601.069,1642.9;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;25;-1493.831,1431.842;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1307.94,1667.823;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;15;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;2;-1513.996,1219.717;Inherit;True;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;23;-1185.458,1434.524;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;3;-877.7277,1270.746;Inherit;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;6;-661.3184,1282.259;Inherit;False;FLOAT3;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewMatrixNode;5;-771.2452,1577.028;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;8;-514.2588,1595.314;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-504.81,1334.582;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;17;-829.5098,-215.1055;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-329.3539,1458.55;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;31;-629.8505,231.5328;Inherit;True;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-215.8104,491.1182;Inherit;False;Property;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;34;-324.4354,312.2735;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;9;-131.0602,1434.549;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-261.4465,1726.941;Inherit;False;Property;_Size;Size;5;0;Create;True;0;0;0;False;0;False;1;0.48;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;192.8411,1166.001;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-61.94427,367.763;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-77.93231,736.1701;Inherit;False;Property;_BaseFlip;Base Flip;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-334.3474,-197.8926;Inherit;False;Property;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;32;299.5944,905.7906;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;119.7442,844.8167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;10;-166.6412,27.70484;Inherit;True;Property;_treetop_1;treetop_1;2;0;Create;True;0;0;0;False;0;False;-1;03ef1576ea6c9a6408fd17110356549c;a129c20482dcb4639a9018768a3e001e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;39;-315.1448,610.0186;Inherit;False;Constant;_Vector1;Vector 1;4;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;18;-502.8891,-74.32999;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-819.4462,740.6668;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2273.18,897.3163;Inherit;False;Property;_radius2;radius2;4;0;Create;True;0;0;0;False;0;False;0;3.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;65;-1622.87,767.4811;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;69;-1907.349,759.1132;Inherit;False;SphereMask;-1;;2;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;41;288.355,622.5333;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;61;-1033.722,651.2609;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;63;-2162.323,965.309;Inherit;False;Property;_hardness2;hardness2;8;0;Create;True;0;0;0;False;0;False;0;2.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1084.587,963.6714;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;0;False;0;False;1.33;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;209.8139,-63.50856;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;66;-1120.848,866.9615;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;64;-1406.264,767.4811;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;20;534.3044,-63.39205;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;UVVertexOffset;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;50;0;47;1
WireConnection;50;1;47;2
WireConnection;50;2;47;3
WireConnection;49;0;46;0
WireConnection;51;15;50;0
WireConnection;51;14;48;0
WireConnection;51;12;49;0
WireConnection;52;1;51;0
WireConnection;53;0;52;0
WireConnection;54;0;53;0
WireConnection;59;0;55;0
WireConnection;59;1;54;0
WireConnection;59;2;56;0
WireConnection;58;0;57;0
WireConnection;58;1;47;4
WireConnection;60;0;59;0
WireConnection;60;1;58;0
WireConnection;71;0;45;0
WireConnection;71;1;60;0
WireConnection;44;0;27;0
WireConnection;44;2;71;0
WireConnection;28;0;44;0
WireConnection;30;0;28;0
WireConnection;23;0;2;0
WireConnection;23;1;25;0
WireConnection;23;2;30;0
WireConnection;3;0;23;0
WireConnection;6;0;3;0
WireConnection;4;0;6;0
WireConnection;4;1;5;0
WireConnection;7;0;4;0
WireConnection;7;1;8;0
WireConnection;31;0;17;0
WireConnection;34;0;31;0
WireConnection;9;0;7;0
WireConnection;21;0;9;0
WireConnection;21;1;22;0
WireConnection;33;0;34;0
WireConnection;33;1;40;0
WireConnection;32;0;33;0
WireConnection;32;1;21;0
WireConnection;43;0;42;0
WireConnection;43;1;60;0
WireConnection;18;0;17;2
WireConnection;67;0;61;0
WireConnection;67;1;66;0
WireConnection;67;2;62;0
WireConnection;65;0;69;0
WireConnection;69;15;50;0
WireConnection;69;14;68;0
WireConnection;69;12;63;0
WireConnection;41;0;39;0
WireConnection;41;1;32;0
WireConnection;41;2;43;0
WireConnection;14;0;13;0
WireConnection;14;1;10;0
WireConnection;66;0;64;0
WireConnection;64;0;65;0
WireConnection;20;0;14;0
WireConnection;20;10;10;4
WireConnection;20;11;41;0
ASEEND*/
//CHKSM=AD231D179A28F184EA70A8EBE535FF09B6DA7E12