using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Windsmoon.SdfFluid.Rendering
{
    internal class SdfFluidRayMarchingPass : ScriptableRenderPass
    {
        #region fields
        private const string RayMarchingPassName = "SDF Fluid Raymarching Pass";
        private static readonly int ParticleBufferId = Shader.PropertyToID("_ParticleBuffer");
        private static readonly int ParticleCountId = Shader.PropertyToID("_ParticleCount");
        private static readonly int SmoothWidthId = Shader.PropertyToID("_SmoothWidth");
        private static readonly int MaxStepsId = Shader.PropertyToID("_MaxSteps");
        private static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");
        private static readonly int StepSafetyId = Shader.PropertyToID("_StepSafety");
        private static readonly int MinStepId = Shader.PropertyToID("_MinStep");
        private static readonly int HitEpsilonId = Shader.PropertyToID("_HitEpsilon");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int AmbientIntensityId = Shader.PropertyToID("_AmbientIntensity");
        private static readonly int SpecularIntensityId = Shader.PropertyToID("_SpecularIntensity");
        private static readonly int SpecularPowerId = Shader.PropertyToID("_SpecularPower");
        private static readonly int FresnelColorId = Shader.PropertyToID("_FresnelColor");
        private static readonly int FresnelIntensityId = Shader.PropertyToID("_FresnelIntensity");
        private static readonly int FresnelPowerId = Shader.PropertyToID("_FresnelPower");
            
        private Material _material;
        private GraphicsBuffer _particleBuffer;
        private int _particleCount;
        private float _smoothWidth;
        private int _maxSteps;
        private float _maxDistance;
        private float _stepSafety;
        private float _minStep;
        private float _hitEpsilon;
        private Color _baseColor;
        private float _ambientIntensity;
        private float _specularIntensity;
        private float _specularPower;
        private Color _fresnelColor;
        private float _fresnelIntensity;
        private float _fresnelPower;
        #endregion

        #region constructors
        public SdfFluidRayMarchingPass()
        {
            ConfigureInput(ScriptableRenderPassInput.Depth);            
        }
        #endregion
        
        #region methods
        public void Setup(
            Material material,
            GraphicsBuffer particleBuffer,
            int particleCount,
            float smoothWidth,
            int maxSteps,
            float maxDistance,
            float stepSafety,
            float minStep,
            float hitEpsilon,
            Color baseColor,
            float ambientIntensity,
            float specularIntensity,
            float specularPower,
            Color fresnelColor,
            float fresnelIntensity,
            float fresnelPower)
        {
            _material = material;
            _particleBuffer = particleBuffer;
            _particleCount = particleCount;
            _smoothWidth = smoothWidth;
            _maxSteps = maxSteps;
            _maxDistance = maxDistance;
            _stepSafety = stepSafety;
            _minStep = minStep;
            _hitEpsilon = hitEpsilon;
            _baseColor = baseColor;
            _ambientIntensity = ambientIntensity;
            _specularIntensity = specularIntensity;
            _specularPower = specularPower;
            _fresnelColor = fresnelColor;
            _fresnelIntensity = fresnelIntensity;
            _fresnelPower = fresnelPower;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            using var builder = renderGraph.AddRasterRenderPass<PassData>(RayMarchingPassName, out var passData);
            
            passData.Material = _material;
            passData.ParticleBuffer = renderGraph.ImportBuffer(_particleBuffer);
            passData.ParticleCount = _particleCount;
            passData.SmoothWidth = _smoothWidth;
            passData.MaxSteps = _maxSteps;
            passData.MaxDistance = _maxDistance;
            passData.StepSafety = _stepSafety;
            passData.MinStep = _minStep;
            passData.HitEpsilon = _hitEpsilon;
            passData.BaseColor = _baseColor;
            passData.AmbientIntensity = _ambientIntensity;
            passData.SpecularIntensity = _specularIntensity;
            passData.SpecularPower = _specularPower;
            passData.FresnelColor = _fresnelColor;
            passData.FresnelIntensity = _fresnelIntensity;
            passData.FresnelPower = _fresnelPower;
            
            builder.UseBuffer(passData.ParticleBuffer, AccessFlags.Read);
            builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc<PassData>(RenderFunc);
        }

        private static void RenderFunc(PassData passData, RasterGraphContext context)
        {
            passData.Material.SetBuffer(ParticleBufferId, passData.ParticleBuffer);
            passData.Material.SetInt(ParticleCountId, passData.ParticleCount);
            passData.Material.SetFloat(SmoothWidthId, passData.SmoothWidth);
            passData.Material.SetInt(MaxStepsId, passData.MaxSteps);
            passData.Material.SetFloat(MaxDistanceId, passData.MaxDistance);
            passData.Material.SetFloat(StepSafetyId, passData.StepSafety);
            passData.Material.SetFloat(MinStepId, passData.MinStep);
            passData.Material.SetFloat(HitEpsilonId, passData.HitEpsilon);
            passData.Material.SetColor(BaseColorId, passData.BaseColor);
            passData.Material.SetFloat(AmbientIntensityId, passData.AmbientIntensity);
            passData.Material.SetFloat(SpecularIntensityId, passData.SpecularIntensity);
            passData.Material.SetFloat(SpecularPowerId, passData.SpecularPower);
            passData.Material.SetColor(FresnelColorId, passData.FresnelColor);
            passData.Material.SetFloat(FresnelIntensityId, passData.FresnelIntensity);
            passData.Material.SetFloat(FresnelPowerId, passData.FresnelPower);
            context.cmd.DrawProcedural(Matrix4x4.identity, passData.Material, 0, MeshTopology.Triangles, 3, 1);
        }
        #endregion
        
        #region nested types
        private class PassData
        {
            #region fields
            public Material Material;
            public BufferHandle ParticleBuffer;
            public int ParticleCount;
            public float SmoothWidth;
            public int MaxSteps;
            public float MaxDistance;
            public float StepSafety;
            public float MinStep;
            public float HitEpsilon;
            public Color BaseColor;
            public float AmbientIntensity;
            public float SpecularIntensity;
            public float SpecularPower;
            public Color FresnelColor;
            public float FresnelIntensity;
            public float FresnelPower;
            #endregion
        }
        #endregion
    }
}
