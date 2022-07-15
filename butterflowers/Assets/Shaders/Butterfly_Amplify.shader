// Upgrade NOTE: upgraded instancing buffer 'Butterfly_Amplify' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Butterfly_Amplify"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.35
		_Color("Color", Color) = (0,0,0,0)
		_DeathColor("Death Color", Color) = (0,0,0,0)
		_Death("Death", Float) = 0
		_NoiseStrength("NoiseStrength", Float) = 1
		_Noise("Noise", Range( 0 , 1)) = 0
		_SinWave("SinWave", Range( 0 , 1)) = 0
		_Vector0("Vector 0", Vector) = (0,0,0,0)
		_TimeOffset("Time Offset", Float) = 0
		_Speed("Speed", Float) = 0
		_MainTexure("Main Texure", 2D) = "white" {}
		_ButterflyAlpha("ButterflyAlpha", 2D) = "white" {}
		_Texture0("Texture 0", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		LOD 100
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float _NoiseStrength;
		uniform float _Noise;
		uniform float3 _Vector0;
		uniform float _SinWave;
		uniform sampler2D _Texture0;
		uniform sampler2D _MainTexure;
		uniform float4 _Color;
		uniform float4 _DeathColor;
		uniform sampler2D _ButterflyAlpha;
		uniform float _Cutoff = 0.35;

		UNITY_INSTANCING_BUFFER_START(Butterfly_Amplify)
			UNITY_DEFINE_INSTANCED_PROP(float, _TimeOffset)
#define _TimeOffset_arr Butterfly_Amplify
			UNITY_DEFINE_INSTANCED_PROP(float, _Speed)
#define _Speed_arr Butterfly_Amplify
			UNITY_DEFINE_INSTANCED_PROP(float, _Death)
#define _Death_arr Butterfly_Amplify
		UNITY_INSTANCING_BUFFER_END(Butterfly_Amplify)


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float _TimeOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_TimeOffset_arr, _TimeOffset);
			float simpleNoise33 = SimpleNoise( (ase_worldPos*1.0 + ( _Time.y + _TimeOffset_Instance )).xy );
			float temp_output_88_0 = (v.texcoord.xy.x*1.0 + -0.5);
			float _Speed_Instance = UNITY_ACCESS_INSTANCED_PROP(_Speed_arr, _Speed);
			float mulTime91 = _Time.y * 6.5;
			float mulTime62 = _Time.y * 26.0;
			float mulTime66 = _Time.y * 13.0;
			float4 appendResult67 = (float4(( ( _Vector0.x * temp_output_88_0 ) * sin( ( ( _Speed_Instance * mulTime91 ) + _TimeOffset_Instance ) ) ) , ( _Vector0.y * sin( ( ( _Speed_Instance * mulTime62 ) + _TimeOffset_Instance ) ) ) , ( _Vector0.z * sin( ( ( _Speed_Instance * mulTime66 ) + _TimeOffset_Instance ) ) ) , 0.0));
			v.vertex.xyz += ( ( ( ( ( simpleNoise33 * _NoiseStrength ) - ( _NoiseStrength * 0.5 ) ) * _Noise ) + ( appendResult67 * _SinWave ) ) * abs( temp_output_88_0 ) ).xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = tex2D( _Texture0, i.uv_texcoord ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float _Death_Instance = UNITY_ACCESS_INSTANCED_PROP(_Death_arr, _Death);
			float4 appendResult17 = (float4(( ( ( tex2D( _MainTexure, ase_screenPosNorm.xy ) * _Color ) * ( 1.0 - _Death_Instance ) ) + ( _Death_Instance * _DeathColor ) ).rgb , 1.0));
			o.Albedo = appendResult17.xyz;
			o.Emission = ( appendResult17 / float4( 5,5,5,5 ) ).xyz;
			o.Alpha = 1;
			clip( tex2D( _ButterflyAlpha, i.uv_texcoord ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
-1.6;170.4;1443.2;783.8;1115.472;729.4348;1.757887;True;False
Node;AmplifyShaderEditor.RangedFloatNode;75;-530.3823,683.6627;Inherit;False;InstancedProperty;_Speed;Speed;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;40;-879.9499,470.2613;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-728.0788,1096.487;Inherit;False;1;0;FLOAT;13;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;91;-731.8218,1230.048;Inherit;False;1;0;FLOAT;6.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;62;-733.0544,810.5121;Inherit;False;1;0;FLOAT;26;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-997.2623,863.2177;Inherit;False;InstancedProperty;_TimeOffset;Time Offset;8;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-67.40307,1372.103;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-676.9238,490.6944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-359.4118,1066.634;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;41;-769.8185,295.3994;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-395.4665,1205.766;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-368.0434,806.4888;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;39;-580.8026,306.4076;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;88;160.2337,1378.606;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-214.3332,1086.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;79;-663.0259,-829.0847;Inherit;True;Property;_MainTexure;Main Texure;10;0;Create;True;0;0;0;False;0;False;None;ff6f3aa6e1be04a159213d070f9b2f04;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;93;-214.7336,1206.52;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;5;-717.7316,-581.951;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;59;-311.0074,595.1086;Inherit;False;Property;_Vector0;Vector 0;7;0;Create;True;0;0;0;False;0;False;0,0,0;2.5,0,5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-221.0937,856.8056;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-27.66174,603.9025;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;33;-322.6732,153.8227;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-732.3694,-318.2278;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.9879739,0.9098039,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-284.8263,395.9449;Inherit;False;Property;_NoiseStrength;NoiseStrength;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-437.0174,-183.3003;Inherit;False;InstancedProperty;_Death;Death;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;65;-32.81117,1010.524;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-325.0976,-606.5066;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;63;-65.77995,884.6108;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;94;-41.0304,1233.424;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-61.68802,432.9461;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;11;-264.9931,-178.2984;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-20.64565,-324.0443;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;99.66311,717.7831;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-57.24755,340.4799;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;98.65793,863.22;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;141.6971,632.8716;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-528.433,-63.78501;Inherit;False;Property;_DeathColor;Death Color;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;42;176.3935,397.5949;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;67;260.4605,785.4935;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-241.266,-48.83233;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;60.92451,524.0151;Inherit;False;Property;_Noise;Noise;5;0;Create;True;0;0;0;False;0;False;0;0.091;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;121.7334,-127.7981;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;50;114.1696,991.4672;Inherit;False;Property;_SinWave;SinWave;6;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;342.6996,403.4931;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;353.4249,43.14868;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;302.7708,-75.30323;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;399.8804,837.8118;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;484.522,-77.9495;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;86;82.05554,248.9742;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;48;501.8828,413.9639;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.AbsOpNode;89;415.331,1378.388;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;82;47.21376,52.75385;Inherit;True;Property;_ButterflyAlpha;ButterflyAlpha;11;0;Create;True;0;0;0;False;0;False;None;e1ce2ebce3a5d415cbdbdc23cc1c5e86;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;98;167.5621,-405.7603;Inherit;True;Property;_Texture0;Texture 0;12;0;Create;True;0;0;0;False;0;False;None;97628c7784bbbd24e83840be8d7d9887;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexCoordVertexDataNode;99;243.6178,-581.3662;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;96;444.0681,-351.4579;Inherit;True;Property;_TextureSample2;Texture Sample 2;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;635.5266,408.3331;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;81;634.2335,94.96771;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;5,5,5,5;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;83;307.1377,158.0833;Inherit;True;Property;_TextureSample1;Texture Sample 1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;792.5607,-79.68934;Float;False;True;-1;2;ASEMaterialInspector;100;0;Standard;Butterfly_Amplify;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.35;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;100;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;71;0;40;0
WireConnection;71;1;74;0
WireConnection;78;0;75;0
WireConnection;78;1;66;0
WireConnection;92;0;75;0
WireConnection;92;1;91;0
WireConnection;77;0;75;0
WireConnection;77;1;62;0
WireConnection;39;0;41;0
WireConnection;39;2;71;0
WireConnection;88;0;38;1
WireConnection;72;0;78;0
WireConnection;72;1;74;0
WireConnection;93;0;92;0
WireConnection;93;1;74;0
WireConnection;73;0;77;0
WireConnection;73;1;74;0
WireConnection;90;0;59;1
WireConnection;90;1;88;0
WireConnection;33;0;39;0
WireConnection;65;0;72;0
WireConnection;7;0;79;0
WireConnection;7;1;5;0
WireConnection;63;0;73;0
WireConnection;94;0;93;0
WireConnection;44;0;35;0
WireConnection;11;0;14;0
WireConnection;10;0;7;0
WireConnection;10;1;1;0
WireConnection;60;0;59;2
WireConnection;60;1;63;0
WireConnection;34;0;33;0
WireConnection;34;1;35;0
WireConnection;64;0;59;3
WireConnection;64;1;65;0
WireConnection;95;0;90;0
WireConnection;95;1;94;0
WireConnection;42;0;34;0
WireConnection;42;1;44;0
WireConnection;67;0;95;0
WireConnection;67;1;60;0
WireConnection;67;2;64;0
WireConnection;15;0;14;0
WireConnection;15;1;3;0
WireConnection;12;0;10;0
WireConnection;12;1;11;0
WireConnection;45;0;42;0
WireConnection;45;1;46;0
WireConnection;13;0;12;0
WireConnection;13;1;15;0
WireConnection;49;0;67;0
WireConnection;49;1;50;0
WireConnection;17;0;13;0
WireConnection;17;3;18;0
WireConnection;48;0;45;0
WireConnection;48;1;49;0
WireConnection;89;0;88;0
WireConnection;96;0;98;0
WireConnection;96;1;99;0
WireConnection;36;0;48;0
WireConnection;36;1;89;0
WireConnection;81;0;17;0
WireConnection;83;0;82;0
WireConnection;83;1;86;0
WireConnection;0;0;17;0
WireConnection;0;1;96;0
WireConnection;0;2;81;0
WireConnection;0;10;83;1
WireConnection;0;11;36;0
ASEEND*/
//CHKSM=7FC281CB07CBFB16941F076D855091DA7F17C807