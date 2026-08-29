Shader "Hidden/Windsmoon/SDF Fluid/Particle Buffer Debug"
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
            Name "ParticleBufferDebug"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct FluidParticleData
            {
                float3 Position;
                float Radius;
                float4 Color;
            };

            struct Varyings
            {
                float4 PositionCS : SV_POSITION;
                float2 CirclePosition : TEXCOORD0;
                float4 Color : COLOR0;
            };

            StructuredBuffer<FluidParticleData> _ParticleBuffer;

            float2 GetQuadCorner(uint vertexId)
            {
                const float2 corners[6] =
                {
                    float2(-1.0, -1.0),
                    float2(-1.0, 1.0),
                    float2(1.0, 1.0),
                    float2(-1.0, -1.0),
                    float2(1.0, 1.0),
                    float2(1.0, -1.0),
                };
                return corners[vertexId];
            }

            Varyings Vert(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
            {
                FluidParticleData particle = _ParticleBuffer[instanceId];
                float2 corner = GetQuadCorner(vertexId);
                float4 centerPositionCS = TransformWorldToHClip(particle.Position);

                // Project a world-space radius directly into clip space. Dividing by W
                // during rasterization makes the debug billboard shrink with distance.
                float2 projectionScale = float2(UNITY_MATRIX_P._m00, UNITY_MATRIX_P._m11);
                centerPositionCS.xy += corner * particle.Radius * projectionScale;

                Varyings output;
                output.PositionCS = centerPositionCS;
                output.CirclePosition = corner;
                output.Color = particle.Color;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                clip(1.0 - dot(input.CirclePosition, input.CirclePosition));
                return input.Color;
            }
            ENDHLSL
        }
    }
}
