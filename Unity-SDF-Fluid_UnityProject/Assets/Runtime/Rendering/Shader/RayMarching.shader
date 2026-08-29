Shader "Hidden/Windsmoon/SDF Fluid/Ray Marching"
{
    Properties
    {
        _SmoothWidth("Smooth Width", Range(0.001, 2.0)) = 0.2
    }

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

            struct FluidParticleData
            {
                float3 position;
                float radius;
                float4 color;
            };
            
            StructuredBuffer<FluidParticleData> _ParticleBuffer;
            int _ParticleCount;
            float _SmoothWidth;
            int _MaxSteps;
            float _StepSafety;
            float _MinStep;
            float _HitEpsilon;
            
            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float SphereSDF(float3 positionWS, float3 sphereCenterWS, float sphereRadius)
            {
                return length(positionWS - sphereCenterWS) - sphereRadius;
            }

            float SmoothMin(float distanceA, float distanceB, float smoothWidth)
            {
                // Only blend where the two distance fields are within smoothWidth.
                float blend = saturate(0.5 + 0.5 * (distanceB - distanceA) / smoothWidth);
                return lerp(distanceB, distanceA, blend) - smoothWidth * blend * (1.0 - blend);
            }
            
            float EvaluateFluidSDF(float3 positionWS)
            {
                float distanceToFluid = 1e20;
                
                for (int i = 0; i < _ParticleCount; ++i)
                {
                    FluidParticleData particleData = _ParticleBuffer[i];
                    float particleDistance = SphereSDF(positionWS, particleData.position, particleData.radius);
                    distanceToFluid = SmoothMin(distanceToFluid, particleDistance, _SmoothWidth);
                }
                
                return distanceToFluid;
            }

            bool RayMarchSphere(float3 rayOriginWS, float3 rayDirectionWS, out float hitDistance)
            {
                const float maxDistance = 100.0;

                hitDistance = 0.0;

                [loop]
                for (int step = 0; step < _MaxSteps; ++step)
                {
                    float3 samplePositionWS = rayOriginWS + rayDirectionWS * hitDistance;
                    float distanceToSurface = EvaluateFluidSDF(samplePositionWS);

                    if (distanceToSurface < _HitEpsilon)
                    {
                        return true;
                    }

                    hitDistance += max(distanceToSurface * _StepSafety, _MinStep);
                    if (hitDistance > maxDistance)
                    {
                        break;
                    }
                }

                return false;
            }

            Varyings Vert(uint vertexId : SV_VertexID)
            {
                Varyings output;
                output.posCS = GetFullScreenTriangleVertexPosition(vertexId);
                output.uv = GetFullScreenTriangleTexCoord(vertexId);
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // Unproject a far-plane point so the ray remains fixed in world space
                // while the camera moves and rotates.
                float3 farPositionWS = ComputeWorldSpacePosition(input.uv,UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
                float3 rayOriginWS = _WorldSpaceCameraPos;
                float3 rayDirectionWS = normalize(farPositionWS - rayOriginWS);

                float hitDistance;
                if (RayMarchSphere(rayOriginWS, rayDirectionWS, hitDistance))
                {
                    return float4(1.0, 0.0, 0.0, 1.0);
                }

                return float4(0.0, 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}
