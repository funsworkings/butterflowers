// Upgrade NOTE: upgraded instancing buffer 'Butterfly_Amplify' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Butterfly_Amplify"
{
	Properties
	{
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
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" }
		LOD 100
		Cull Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform float _NoiseStrength;
		uniform float _Noise;
		uniform float3 _Vector0;
		uniform float _SinWave;
		uniform sampler2D _MainTexure;
		uniform float4 _Color;
		uniform float4 _DeathColor;

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
			float _Speed_Instance = UNITY_ACCESS_INSTANCED_PROP(_Speed_arr, _Speed);
			float mulTime62 = _Time.y * 26.0;
			float mulTime66 = _Time.y * 13.0;
			float4 appendResult67 = (float4(( _Vector0.x * sin( ( ( _Speed_Instance * mulTime62 ) + _TimeOffset_Instance ) ) ) , 0.0 , ( _Vector0.z * sin( ( ( _Speed_Instance * mulTime66 ) + _TimeOffset_Instance ) ) ) , 0.0));
			v.vertex.xyz += ( ( ( ( ( simpleNoise33 * _NoiseStrength ) - ( _NoiseStrength * 0.5 ) ) * _Noise ) + ( appendResult67 * _SinWave ) ) * v.texcoord.xy.y ).xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float _Death_Instance = UNITY_ACCESS_INSTANCED_PROP(_Death_arr, _Death);
			float4 appendResult17 = (float4(( ( ( tex2D( _MainTexure, ase_screenPosNorm.xy ) * _Color ) * ( 1.0 - _Death_Instance ) ) + ( _Death_Instance * _DeathColor ) ).rgb , 1.0));
			o.Albedo = appendResult17.xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
1108;366;477;713;618.798;616.627;1.507657;False;False
Node;AmplifyShaderEditor.SimpleTimeNode;40;-879.9499,470.2613;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-969.8977,729.6133;Inherit;False;InstancedProperty;_TimeOffset;Time Offset;9;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;62;-666.9365,992.3807;Inherit;False;1;0;FLOAT;26;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-592.1509,813.458;Inherit;False;InstancedProperty;_Speed;Speed;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-546.4647,1155.538;Inherit;False;1;0;FLOAT;13;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-479.1127,919.1676;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-361.0215,1095.609;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;41;-790.3254,320.6386;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-676.9238,490.6944;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-214.3332,1086.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-351.4796,926.0229;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;39;-580.8026,306.4076;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;59;-166.9441,550.4006;Inherit;False;Property;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SinOpNode;65;-32.81117,1010.524;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;63;-66.0959,863.4875;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-259.9331,434.5294;Inherit;False;Property;_NoiseStrength;NoiseStrength;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;33;-322.6732,152.243;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;5;-717.7316,-581.951;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;79;-713.5044,-982.0976;Inherit;True;Property;_MainTexure;Main Texure;11;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;86.8655,716.1835;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-33.69441,316.9267;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-22.43278,432.9461;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;98.65793,863.22;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-732.3694,-318.2278;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-325.0976,-606.5066;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-437.0174,-183.3003;Inherit;False;InstancedProperty;_Death;Death;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;11;-264.9931,-178.2984;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-20.64565,-324.0443;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;3;-534.7657,18.5397;Inherit;False;Property;_DeathColor;Death Color;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;42;177.9732,364.421;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;32.51861,1305.509;Inherit;False;Property;_SinWave;SinWave;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;67;260.4605,785.4935;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;115.0658,536.4135;Inherit;False;Property;_Noise;Noise;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;383.5251,439.6081;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-241.266,-48.83233;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;399.8804,837.8118;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;121.7334,-127.7981;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;577.5223,837.1804;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;13;302.7708,-75.30323;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;18;353.4249,43.14868;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;48;514.4445,539.5811;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;20;474.1564,-449.0392;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;17;484.522,-77.9495;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;658.9427,524.431;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;19;206.3867,-705.6052;Inherit;True;Property;_Texture1;Texture 1;4;0;Create;True;0;0;0;False;0;False;a1ba4fa9fc72b784585f4724634891a7;a1ba4fa9fc72b784585f4724634891a7;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;796.912,-94.91865;Float;False;True;-1;2;ASEMaterialInspector;100;0;Standard;Butterfly_Amplify;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;100;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;77;0;75;0
WireConnection;77;1;62;0
WireConnection;78;0;75;0
WireConnection;78;1;66;0
WireConnection;71;0;40;0
WireConnection;71;1;74;0
WireConnection;72;0;78;0
WireConnection;72;1;74;0
WireConnection;73;0;77;0
WireConnection;73;1;74;0
WireConnection;39;0;41;0
WireConnection;39;2;71;0
WireConnection;65;0;72;0
WireConnection;63;0;73;0
WireConnection;33;0;39;0
WireConnection;60;0;59;1
WireConnection;60;1;63;0
WireConnection;34;0;33;0
WireConnection;34;1;35;0
WireConnection;44;0;35;0
WireConnection;64;0;59;3
WireConnection;64;1;65;0
WireConnection;7;0;79;0
WireConnection;7;1;5;0
WireConnection;11;0;14;0
WireConnection;10;0;7;0
WireConnection;10;1;1;0
WireConnection;42;0;34;0
WireConnection;42;1;44;0
WireConnection;67;0;60;0
WireConnection;67;2;64;0
WireConnection;45;0;42;0
WireConnection;45;1;46;0
WireConnection;15;0;14;0
WireConnection;15;1;3;0
WireConnection;49;0;67;0
WireConnection;49;1;50;0
WireConnection;12;0;10;0
WireConnection;12;1;11;0
WireConnection;13;0;12;0
WireConnection;13;1;15;0
WireConnection;48;0;45;0
WireConnection;48;1;49;0
WireConnection;20;0;19;0
WireConnection;17;0;13;0
WireConnection;17;3;18;0
WireConnection;36;0;48;0
WireConnection;36;1;38;2
WireConnection;0;0;17;0
WireConnection;0;11;36;0
ASEEND*/
//CHKSM=6B42A372A04514A88714FB6B2D0C16652DCE6909