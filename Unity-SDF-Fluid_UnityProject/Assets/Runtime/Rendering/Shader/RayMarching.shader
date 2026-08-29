Shader "Hidden/Windsmoon/SDF Fluid/Ray Marching"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "RayMarching"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings
            {
                float4 PositionCS : SV_POSITION;
                float2 UV : TEXCOORD0;
            };

            Varyings Vert(uint vertexId : SV_VertexID)
            {
                Varyings output;
                output.PositionCS = GetFullScreenTriangleVertexPosition(vertexId);
                output.UV = GetFullScreenTriangleTexCoord(vertexId);
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // UV gradient verifies that the RenderGraph pass covers the full target.
                return float4(input.UV, 0.0, 1.0);
            }
            ENDHLSL
        }
    }
}
