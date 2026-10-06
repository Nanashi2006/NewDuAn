Shader "Chapter14/RainParticles"
{
    Properties { _BaseColor ("Rain Tint", Color) = (0.7,0.84,1,0.55) }
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
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float fog:TEXCOORD1; };
            V Vert(A v)
            {
                V o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv; o.color = v.color * _BaseColor;
                o.fog = ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 Frag(V i):SV_Target
            {
                float alpha = saturate(1 - abs(i.uv.x - 0.5) * 2);
                alpha *= smoothstep(0,0.12,i.uv.y) * smoothstep(0,0.12,1-i.uv.y);
                return half4(MixFog(i.color.rgb,i.fog),i.color.a * alpha);
            }
            ENDHLSL
        }
    }
}
