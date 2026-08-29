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
        private static readonly int StepSafetyId = Shader.PropertyToID("_StepSafety");
        private static readonly int MinStepId = Shader.PropertyToID("_MinStep");
        private static readonly int HitEpsilonId = Shader.PropertyToID("_HitEpsilon");
            
        private Material _material;
        private GraphicsBuffer _particleBuffer;
        private int _particleCount;
        private float _smoothWidth;
        private int _maxSteps;
        private float _stepSafety;
        private float _minStep;
        private float _hitEpsilon;
        #endregion
        
        #region methods

        public void Setup(Material material, GraphicsBuffer particleBuffer, int particleCount, float smoothWidth, int maxSteps, float stepSafety, float minStep, float hitEpsilon)
        {
            _material = material;
            _particleBuffer = particleBuffer;
            _particleCount = particleCount;
            _smoothWidth = smoothWidth;
            _maxSteps = maxSteps;
            _stepSafety = stepSafety;
            _minStep = minStep;
            _hitEpsilon = hitEpsilon;
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
            passData.StepSafety = _stepSafety;
            passData.MinStep = _minStep;
            passData.HitEpsilon = _hitEpsilon;
            
            builder.UseBuffer(passData.ParticleBuffer, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc<PassData>(RenderFunc);
        }

        private static void RenderFunc(PassData passData, RasterGraphContext context)
        {
            passData.Material.SetBuffer(ParticleBufferId, passData.ParticleBuffer);
            passData.Material.SetInt(ParticleCountId, passData.ParticleCount);
            passData.Material.SetFloat(SmoothWidthId, passData.SmoothWidth);
            passData.Material.SetInt(MaxStepsId, passData.MaxSteps);
            passData.Material.SetFloat(StepSafetyId, passData.StepSafety);
            passData.Material.SetFloat(MinStepId, passData.MinStep);
            passData.Material.SetFloat(HitEpsilonId, passData.HitEpsilon);
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
            public float StepSafety;
            public float MinStep;
            public float HitEpsilon;
            #endregion
        }
        #endregion
    }
}
