//
//  OutlineMask.shader
//  QuickOutline
//
//  Created by Chris Nolet on 2/21/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

Shader "Custom/Outline Mask" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    _GridUnits("Grid Units", Float) = 7
    _InteractionRadius("Interaction Radius", Float) = 2
    _hardness("hardness", Float) = 0.57
    _Displacement("Displacement", Float) = 0.5
    _PlayerPos("PlayerPos", Vector) = (0,0,0,0)
  }

  SubShader {
    Tags {
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }
      Cull Off
        CGINCLUDE
        #include "UnityShaderVariables.cginc"
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0

      struct Input
        {
            float3 worldPos;
            float2 uv_texcoord;
        };

      uniform float _GridUnits;
        uniform float4 _PlayerPos;
        uniform float _InteractionRadius;
        uniform float _hardness;
        uniform float _Displacement;


      

      void vertexDataFunc(inout appdata_full v, out Input o)
      {
          UNITY_INITIALIZE_OUTPUT(Input, o);
          float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex);
          float3 appendResult43 = (float3(_PlayerPos.x, _PlayerPos.y, _PlayerPos.z));
          float3 temp_output_5_0_g1 = ((ase_worldPos - appendResult43) / _InteractionRadius);
          float dotResult8_g1 = dot(temp_output_5_0_g1, temp_output_5_0_g1);
          float clampResult48 = clamp(distance(0.0, pow(saturate(dotResult8_g1), (1.0 - _hardness))), 0.0, 1.0);
          float temp_output_55_0 = ((3.0 * (1.0 - clampResult48) * 10.0) * (_Displacement * _PlayerPos.w));
          float mulTime72 = _Time.y * -0.1;
          float temp_output_69_0 = (_GridUnits + (temp_output_55_0 * cos(mulTime72)));
          v.vertex.xyz += ((round((ase_worldPos * temp_output_69_0)) / temp_output_69_0) - ase_worldPos);
          v.vertex.w = 1;
      }
      ENDCG
          CGPROGRAM
        #pragma vertex:vertexDataFunc 
    Pass {
      Name "Mask"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      ColorMask 0
      CGPROGRAM
      #pragma vertex vert

          struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 customPack1 : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 tSpace0 : TEXCOORD3;
                float4 tSpace1 : TEXCOORD4;
                float4 tSpace2 : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                Input customInputData;
                vertexDataFunc(v, customInputData);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                o.customPack1.xy = customInputData.uv_texcoord;
                o.customPack1.xy = v.texcoord;
                o.worldPos = worldPos;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

      Stencil {
        Ref 1
        Pass Replace
      }
          ENDCG
    }
  }
}
