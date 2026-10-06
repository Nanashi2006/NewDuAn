Shader "Chapter14/AssetLit"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        [Normal] _BumpMap ("Normal", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Float) = 1
        _Smoothness ("Smoothness", Range(0,1)) = 0.3
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.4
        _AlphaClip ("Alpha Clip", Float) = 0
        _Cull ("Cull", Float) = 2
        _EmissionColor ("Emission", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Cull [_Cull]
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor, _BaseMap_ST, _EmissionColor;
            float _Smoothness, _Cutoff, _AlphaClip, _Cull, _BumpScale;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 tangentOS:TANGENT; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float4 tangentWS:TEXCOORD2; float2 uv:TEXCOORD3; float fog:TEXCOORD4; };
            V Vert(A v)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.tangentWS = float4(TransformObjectToWorldDir(v.tangentOS.xyz), v.tangentOS.w * GetOddNegativeScale());
                o.uv = TRANSFORM_TEX(v.uv,_BaseMap);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            half4 Frag(V i):SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv) * _BaseColor;
                if (_AlphaClip > 0.5) clip(baseSample.a - _Cutoff);
                float3 n = normalize(i.normalWS);
                if (dot(i.tangentWS.xyz,i.tangentWS.xyz) > 0.01)
                {
                    float3 tangent = normalize(i.tangentWS.xyz);
                    float3 bitangent = cross(n,tangent) * i.tangentWS.w;
                    float3 map = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
                    n = normalize(tangent * map.x + bitangent * map.y + n * map.z);
                }
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 result = baseSample.rgb * (SampleSH(n) + sun.color * saturate(dot(n,sun.direction)) * sun.shadowAttenuation);
                float3 h = normalize(sun.direction + GetWorldSpaceNormalizeViewDir(i.positionWS));
                result += sun.color * pow(saturate(dot(n,h)),lerp(8,128,_Smoothness)) * _Smoothness * 0.3 * sun.shadowAttenuation;
                #ifdef _ADDITIONAL_LIGHTS
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                uint count = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                {
                    Light l = GetAdditionalLight(lightIndex,i.positionWS);
                    result += baseSample.rgb * l.color * saturate(dot(n,l.direction)) * l.distanceAttenuation;
                }
                LIGHT_LOOP_END
                #endif
                return half4(MixFog(result + _EmissionColor.rgb,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}
