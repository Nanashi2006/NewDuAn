Shader "Chapter14/LakeWater"
{
    Properties
    {
        _BaseColor ("Water", Color) = (0.07,0.42,0.45,0.8)
        _BaseMap ("Ripple texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor, _BaseMap_ST;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float fog:TEXCOORD1; };
            V Vert(A v)
            {
                V o;
                float3 p = TransformObjectToWorld(v.positionOS.xyz);
                p.y += sin(p.x * 1.8 + _Time.y * 1.4) * sin(p.z * 1.2 - _Time.y) * 0.035;
                o.positionWS = p; o.positionCS = TransformWorldToHClip(p);
                o.fog = ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 Frag(V i):SV_Target
            {
                float2 uv = i.positionWS.xz + _BaseMap_ST.zw * 18;
                float3 n = normalize(float3(cos(uv.x * 1.8 + _Time.y * 1.4) * 0.1,1,sin(uv.y * 1.2 - _Time.y) * 0.08));
                float3 view = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float fresnel = pow(1 - saturate(dot(view,n)),3);
                Light sun = GetMainLight();
                float spec = pow(saturate(dot(n,normalize(view + sun.direction))),160);
                float ring = length((i.positionWS.xz - float2(12,10)) / float2(17,13));
                float foam = smoothstep(0.82,0.89,ring) * (0.6 + sin(ring * 190 - _Time.y * 1.5) * 0.25);
                float ripple = pow(saturate(sin(uv.x * 2 + sin(uv.y * 1.3 + _Time.y))),20) * 0.12;
                float3 color = lerp(_BaseColor.rgb,float3(0.42,0.69,0.73),fresnel) + ripple + foam * 0.3;
                color *= max(0.15,sun.color * sun.distanceAttenuation * 0.7 + SampleSH(n));
                color += spec * sun.color * 0.6;
                return half4(MixFog(color,i.fog),_BaseColor.a);
            }
            ENDHLSL
        }
    }
}
