Shader "Custom/MaskedURP_3D"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _ZWrite ("ZWrite", Float) = 0

        // 3 = Equal, 8 = Always (Unity CompareFunction enum)
        [Enum(Equal,3, Always,8)] _StencilComp ("Stencil Comp", Float) = 3
        _StencilRef ("Stencil Ref", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "Masked3D"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass Keep
            }

            Cull Off
            ZWrite [_ZWrite]
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return texCol * IN.color * _BaseColor;
            }
            ENDHLSL
        }
    }
}
