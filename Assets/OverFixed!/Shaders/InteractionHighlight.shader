// 상호작용 대상의 외곽선 빛. 대상 메시를 법선 방향으로 부풀려 뒷면만 그린다 (앞면 뒤로 가려져 테두리만 남음)
// 덧셈 섞기라 어두운 곳에서 빛처럼 보인다. 밝기(_Intensity)는 재질에서 정한다 (모든 대상 같은 밝기)
Shader "OverFixed/InteractionHighlight"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.65, 0.23, 1)
        _Intensity ("Intensity", Float) = 1
        _OutlineWidth ("Outline Width (m)", Range(0, 0.1)) = 0.025
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                positionWS += TransformObjectToWorldNormal(input.normalOS) * _OutlineWidth;
                return TransformWorldToHClip(positionWS);
            }

            half4 frag() : SV_Target
            {
                return half4(_Color.rgb * _Intensity, 1.0h);
            }
            ENDHLSL
        }
    }
}
