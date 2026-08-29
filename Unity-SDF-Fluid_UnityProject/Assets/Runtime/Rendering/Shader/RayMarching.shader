Shader "Hidden/Windsmoon/SDF Fluid/Ray Marching"
{
    Properties
    {
        _SmoothWidth("Smooth Width", Range(0.001, 2.0)) = 0.2
        _BaseColor("Base Color", Color) = (0.05, 0.35, 0.8, 1.0)
        _AmbientIntensity("Ambient Intensity", Range(0.0, 1.0)) = 0.15
        _SpecularIntensity("Specular Intensity", Range(0.0, 4.0)) = 1.0
        _SpecularPower("Specular Power", Range(1.0, 256.0)) = 64.0
        _FresnelColor("Fresnel Color", Color) = (0.7, 0.9, 1.0, 1.0)
        _FresnelIntensity("Fresnel Intensity", Range(0.0, 4.0)) = 0.5
        _FresnelPower("Fresnel Power", Range(0.5, 8.0)) = 4.0
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
            Name "Ray Marching"
            Blend One Zero
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

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
            float _MaxDistance;
            float _StepSafety;
            float _MinStep;
            float _HitEpsilon;
            float4 _BaseColor;
            float _AmbientIntensity;
            float _SpecularIntensity;
            float _SpecularPower;
            float4 _FresnelColor;
            float _FresnelIntensity;
            float _FresnelPower;
            
            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Output
            {
                float4 fluidColor : SV_TARGET0;
                float4 sceneDepth : SV_TARGET1;
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

            float3 EstimateFluidNormal(float3 positionWS)
            {
                // Sample the fused field on both sides of the hit point so the
                // gradient remains stable across Smooth Min particle seams.
                float normalEpsilon = max(_HitEpsilon, 0.0001);
                float3 offsetX = float3(normalEpsilon, 0.0, 0.0);
                float3 offsetY = float3(0.0, normalEpsilon, 0.0);
                float3 offsetZ = float3(0.0, 0.0, normalEpsilon);
                float3 gradient = float3(
                    EvaluateFluidSDF(positionWS + offsetX) - EvaluateFluidSDF(positionWS - offsetX),
                    EvaluateFluidSDF(positionWS + offsetY) - EvaluateFluidSDF(positionWS - offsetY),
                    EvaluateFluidSDF(positionWS + offsetZ) - EvaluateFluidSDF(positionWS - offsetZ));
                return normalize(gradient);
            }

            float3 ShadeFluid(float3 hitPositionWS, float3 surfaceNormalWS)
            {
                Light mainLight = GetMainLight();
                float diffuseIntensity = saturate(dot(surfaceNormalWS, mainLight.direction));
                // A procedural full-screen draw has no Renderer to populate
                // unity_LightData.z, so GetMainLight().distanceAttenuation is
                // not a valid main-light culling value for this pass.
                float3 directLighting = mainLight.color * diffuseIntensity;
                float3 viewDirectionWS = normalize(_WorldSpaceCameraPos - hitPositionWS);
                float3 halfDirectionWS = SafeNormalize(mainLight.direction + viewDirectionWS);
                float specularIntensity = pow(saturate(dot(surfaceNormalWS, halfDirectionWS)), _SpecularPower) * step(0.0001, diffuseIntensity);
                float3 specularLighting = mainLight.color * specularIntensity * _SpecularIntensity;
                float fresnelIntensity = pow(1.0 - saturate(dot(surfaceNormalWS, viewDirectionWS)), _FresnelPower) * _FresnelIntensity;
                float3 fresnelLighting = _FresnelColor.rgb * fresnelIntensity;
                return _BaseColor.rgb * (_AmbientIntensity + directLighting) + specularLighting + fresnelLighting;
            }

            bool RayMarchSphere(float3 rayOriginWS, float3 rayDirectionWS, float maxDistance, out float hitDistance)
            {
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
                    if (hitDistance >= maxDistance)
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
            
            
            Output Frag(Varyings input)
            {
                float rawSceneDepth = SampleSceneDepth(input.uv);
                float sceneDepth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);
                
#if UNITY_REVERSED_Z
                float sceneDeviceDepth = rawSceneDepth;
#else
                // ComputeWorldSpacePosition expects the platform's NDC depth range.
                float sceneDeviceDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, rawSceneDepth);
#endif
                
                // Unproject a far-plane point so the ray remains fixed in world space
                // while the camera moves and rotates.
                float3 farPositionWS = ComputeWorldSpacePosition(input.uv,UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
                
                float3 rayOriginWS = _WorldSpaceCameraPos;
                float3 rayDirectionWS = normalize(farPositionWS - rayOriginWS);
                float3 scenePositionWS = ComputeWorldSpacePosition(input.uv, sceneDeviceDepth, UNITY_MATRIX_I_VP);
                float sceneDepthT = dot(scenePositionWS - rayOriginWS, rayDirectionWS);
                float maxRayMarchDistance = min(_MaxDistance, sceneDepthT);
                
                Output output;
                output.sceneDepth = sceneDepth;
                output.fluidColor = float4(0.0, 0.0, 0.0, 0.0);

                float hitDistance;
                if (RayMarchSphere(rayOriginWS, rayDirectionWS, maxRayMarchDistance, hitDistance))
                {
                    float3 hitPositionWS = rayOriginWS + rayDirectionWS * hitDistance;
                    float3 surfaceNormalWS = EstimateFluidNormal(hitPositionWS);
                    float3 surfaceColor = ShadeFluid(hitPositionWS, surfaceNormalWS);
                    output.fluidColor = float4(surfaceColor, _BaseColor.a);
                }
                
                return output;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Composite"

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex VertComposite
            #pragma fragment FragComposite

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_HalfResolutionColorTexture);
            TEXTURE2D(_HalfResolutionSceneDepthTexture);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            Varyings VertComposite(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }
            
            half4 SampleDepthAwareFluid(float2 uv)
            {
                float rawDepth = SampleSceneDepth(uv);
                float fullSceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                
                uint halfWidth;
                uint halfHeight;
                _HalfResolutionSceneDepthTexture.GetDimensions(halfWidth, halfHeight);
                
                float2 texelPosition = uv * float2(halfWidth, halfHeight) - 0.5;
                int2 baseCoord = int2(floor(texelPosition));
                int2 maxCoord = int2(halfWidth, halfHeight) - 1;
                
                float minDepthDiff = 1e20;
                int2 selectedCoord = int2(0, 0);
                
                [unroll]
                for (int y = 0; y < 2; y++)
                {
                    [unroll]
                    for (int x = 0; x < 2; x++)
                    {
                        int2 coord = clamp(baseCoord + int2(x, y), int2(0, 0), maxCoord);
                        float candidateDepth = LOAD_TEXTURE2D(_HalfResolutionSceneDepthTexture, coord).r;
                        float depthDiff = abs(candidateDepth - fullSceneDepth);
                        
                        if (depthDiff < minDepthDiff)
                        {
                            selectedCoord = coord;
                            minDepthDiff = depthDiff;
                        }
                    }
                }
                
                return LOAD_TEXTURE2D(_HalfResolutionColorTexture, selectedCoord);
            }

            half4 FragComposite(Varyings input) : SV_Target
            {
                return SampleDepthAwareFluid(input.texcoord);
            }
            ENDHLSL
        }
    }
}
