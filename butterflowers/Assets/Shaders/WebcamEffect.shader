// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WebcamEffect"
{
	Properties
	{
		_Blur("Blur", Range( 0 , 0.05)) = 0.04215152
		_Distortion("Distortion", Range( 0 , 1)) = 1
		_Texture0("Texture 0", 2D) = "white" {}
		_Brightness("Brightness", Range( 1 , 50)) = 1
		_Texture1("Texture 1", 2D) = "white" {}
		_StarSpeed("Star Speed", Range( 0 , 1)) = 0.3
		_StarTiling("Star Tiling", Range( 0 , 50)) = 0
		_StarVisibility("Star Visibility", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform sampler2D _Texture0;
		uniform float _Distortion;
		uniform float _Blur;
		uniform sampler2D _Texture1;
		uniform float _StarTiling;
		uniform float _StarSpeed;
		uniform float _Brightness;
		uniform float _StarVisibility;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float2 voronoihash18( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi18( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash18( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return F1;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (i.uv_texcoord.x).xx;
			float mulTime34 = _Time.y * 0.04;
			float simplePerlin2D27 = snoise( (i.uv_texcoord*1.0 + mulTime34)*0.5 );
			simplePerlin2D27 = simplePerlin2D27*0.5 + 0.5;
			float mulTime37 = _Time.y * -0.066;
			float simplePerlin2D35 = snoise( (i.uv_texcoord*1.0 + mulTime37) );
			simplePerlin2D35 = simplePerlin2D35*0.5 + 0.5;
			float2 lerpResult30 = lerp( i.uv_texcoord , temp_cast_0 , ( ( simplePerlin2D27 * simplePerlin2D35 ) * _Distortion ));
			float time18 = 0.0;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 coords18 = ase_screenPosNorm.xy * 10000.0;
			float2 id18 = 0;
			float2 uv18 = 0;
			float voroi18 = voronoi18( coords18, time18, id18, uv18, 0 );
			float2 lerpResult42 = lerp( lerpResult30 , id18 , _Blur);
			float4 tex2DNode2 = tex2D( _Texture0, lerpResult42 );
			float2 temp_output_55_0 = (lerpResult42*_StarTiling + 0.0);
			float mulTime73 = _Time.y * _StarSpeed;
			float4 appendResult78 = (float4(0.0 , mulTime73 , 0.0 , 0.0));
			float mulTime66 = _Time.y * _StarSpeed;
			float simplePerlin2D62 = snoise( ( ( floor( temp_output_55_0 ) + ( round( mulTime66 ) / _StarSpeed ) ) / _StarTiling )*_StarTiling );
			simplePerlin2D62 = simplePerlin2D62*0.5 + 0.5;
			float smoothstepResult69 = smoothstep( 0.1 , 1.0 , ( ( tex2D( _Texture1, frac( temp_output_55_0 ) ).a * tex2D( _Texture1, frac( (lerpResult42*_StarTiling + appendResult78.xy) ) ).a ) * step( 0.8 , simplePerlin2D62 ) ));
			float4 lerpResult89 = lerp( tex2DNode2 , ( tex2DNode2 + ( smoothstepResult69 * _Brightness ) ) , _StarVisibility);
			o.Emission = ( lerpResult89 * _Brightness ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
7.2;325.6;875;443.2;1206.3;-1112.862;1.032732;True;False
Node;AmplifyShaderEditor.TexCoordVertexDataNode;19;-2754.635,59.24982;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;37;-2351.749,-279.2149;Inherit;False;1;0;FLOAT;-0.066;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-2315.077,-411.9863;Inherit;False;1;0;FLOAT;0.04;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;36;-2058.975,-305.3863;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;33;-2061.92,-492.3447;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;35;-1840.576,-276.7863;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;27;-1837.56,-525.3837;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1588.817,-352.6538;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1909.577,-29.54791;Inherit;False;Property;_Distortion;Distortion;1;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1275.168,-71.22268;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;47;-2097.416,310.4394;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-1313.04,316.4048;Inherit;False;Property;_Blur;Blur;0;0;Create;True;0;0;0;False;0;False;0.04215152;0;0;0.05;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;18;-1779.45,227.597;Inherit;False;0;0;1;0;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;10000;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.LerpOp;30;-1221.489,63.94393;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-2449.433,989.4417;Inherit;False;Property;_StarSpeed;Star Speed;5;0;Create;True;0;0;0;False;0;False;0.3;0.499;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-2270.981,671.7917;Inherit;False;Property;_StarTiling;Star Tiling;6;0;Create;True;0;0;0;False;0;False;0;10;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;42;-969.2734,86.72409;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-1961.73,1492.298;Inherit;False;1;0;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;73;-2265.3,839.2023;Inherit;False;1;0;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;55;-1846.645,547.9487;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;10;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RoundOpNode;81;-1734.843,1319.318;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;82;-1491.761,1613.882;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;78;-2053.105,824.2958;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FloorOpNode;64;-1480.42,1266.333;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-1192.529,1565.379;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;71;-1849.526,804.8671;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT;10;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;61;-1170.393,1219.753;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;54;-1441.076,617.6633;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;57;-1515.009,396.7469;Inherit;True;Property;_Texture1;Texture 1;4;0;Create;True;0;0;0;False;0;False;None;6541c673d674b0c47a31b4867e8e3cbd;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FractNode;75;-1395.495,869.9956;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;56;-1006.23,542.2844;Inherit;True;Property;_TextureSample1;Texture Sample 1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;62;-890.7585,1299.264;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;76;-1014.627,779.308;Inherit;True;Property;_TextureSample2;Texture Sample 1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;63;-618.012,1237.176;Inherit;True;2;0;FLOAT;0.8;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-638.6349,641.0197;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-401.12,722.0192;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;69;-205.0435,610.6376;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;45;-934.0886,-264.4872;Inherit;True;Property;_Texture0;Texture 0;2;0;Create;True;0;0;0;False;0;False;50f45dd4ee2699c4fbed5fc1f0df135d;50f45dd4ee2699c4fbed5fc1f0df135d;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;43;65.72652,324.8142;Inherit;False;Property;_Brightness;Brightness;3;0;Create;True;0;0;0;False;0;False;1;1;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-565.1486,0.5301759;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;90.23869,684.3083;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;-448.2532,398.5108;Inherit;False;Property;_StarVisibility;Star Visibility;7;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;-243.345,185.8351;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;89;1.100593,117.8933;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;451.6487,155.9346;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;883.8738,77.39112;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;WebcamEffect;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;36;0;19;0
WireConnection;36;2;37;0
WireConnection;33;0;19;0
WireConnection;33;2;34;0
WireConnection;35;0;36;0
WireConnection;27;0;33;0
WireConnection;38;0;27;0
WireConnection;38;1;35;0
WireConnection;40;0;38;0
WireConnection;40;1;39;0
WireConnection;18;0;47;0
WireConnection;30;0;19;0
WireConnection;30;1;19;1
WireConnection;30;2;40;0
WireConnection;42;0;30;0
WireConnection;42;1;18;1
WireConnection;42;2;3;0
WireConnection;66;0;85;0
WireConnection;73;0;85;0
WireConnection;55;0;42;0
WireConnection;55;1;86;0
WireConnection;81;0;66;0
WireConnection;82;0;81;0
WireConnection;82;1;85;0
WireConnection;78;1;73;0
WireConnection;64;0;55;0
WireConnection;67;0;64;0
WireConnection;67;1;82;0
WireConnection;71;0;42;0
WireConnection;71;1;86;0
WireConnection;71;2;78;0
WireConnection;61;0;67;0
WireConnection;61;1;86;0
WireConnection;54;0;55;0
WireConnection;75;0;71;0
WireConnection;56;0;57;0
WireConnection;56;1;54;0
WireConnection;62;0;61;0
WireConnection;62;1;86;0
WireConnection;76;0;57;0
WireConnection;76;1;75;0
WireConnection;63;1;62;0
WireConnection;65;0;56;4
WireConnection;65;1;76;4
WireConnection;79;0;65;0
WireConnection;79;1;63;0
WireConnection;69;0;79;0
WireConnection;2;0;45;0
WireConnection;2;1;42;0
WireConnection;91;0;69;0
WireConnection;91;1;43;0
WireConnection;90;0;2;0
WireConnection;90;1;91;0
WireConnection;89;0;2;0
WireConnection;89;1;90;0
WireConnection;89;2;88;0
WireConnection;49;0;89;0
WireConnection;49;1;43;0
WireConnection;0;2;49;0
ASEEND*/
//CHKSM=5A3939E48B54FB91F4CF3B1876305DA36B152199