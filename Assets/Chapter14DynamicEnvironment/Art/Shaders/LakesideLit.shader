Shader "Chapter14/LakesideLit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Glow", Color) = (0,0,0,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.25
        _Ground ("Landscape blending", Float) = 0
        _BaseMap ("Texture", 2D) = "white" {}
        _GroundMap ("Grass texture", 2D) = "white" {}
        _BankMap ("Bank texture", 2D) = "white" {}
        _TextureWeight ("Texture blend", Range(0,1)) = 0
        [HideInInspector] _Cutoff ("Cutoff", Float) = 0.5
        [HideInInspector] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_GroundMap); SAMPLER(sampler_GroundMap);
            TEXTURE2D(_BankMap); SAMPLER(sampler_BankMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor, _EmissionColor, _BaseMap_ST;
            float _Smoothness, _Ground, _Cutoff, _Cull, _TextureWeight;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float fog:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 color = _BaseColor.rgb;
                if (_Ground > 0.5)
                {
                    float r = length((i.positionWS.xz - float2(12,10)) / float2(17,13));
                    float noise = sin(i.positionWS.x * 0.31) * sin(i.positionWS.z * 0.24);
                    float3 grass = lerp(float3(0.22,0.37,0.16), float3(0.38,0.49,0.22), noise * 0.5 + 0.5);
                    float bank = 1 - smoothstep(0.88,1.17,r);
                    grass = lerp(grass, SAMPLE_TEXTURE2D(_GroundMap,sampler_GroundMap,i.positionWS.xz / 6).rgb, _TextureWeight);
                    float3 dirt = lerp(float3(0.59,0.48,0.31), SAMPLE_TEXTURE2D(_BankMap,sampler_BankMap,i.positionWS.xz / 5).rgb, _TextureWeight);
                    color = lerp(grass, dirt, bank);
                    color = lerp(color,float3(0.21,0.31,0.29),saturate(-i.positionWS.y / 3));
                }
                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 result = color * (SampleSH(n) + light.color * saturate(dot(n,light.direction)) * light.shadowAttenuation);
                float3 h = normalize(light.direction + GetWorldSpaceNormalizeViewDir(i.positionWS));
                result += light.color * pow(saturate(dot(n,h)),lerp(8,100,_Smoothness)) * _Smoothness * 0.25 * light.shadowAttenuation;
                #ifdef _ADDITIONAL_LIGHTS
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                uint count = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                {
                    Light l = GetAdditionalLight(lightIndex,i.positionWS);
                    result += color * l.color * saturate(dot(n,l.direction)) * l.distanceAttenuation;
                }
                LIGHT_LOOP_END
                #endif
                return half4(MixFog(result + _EmissionColor.rgb, i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}
