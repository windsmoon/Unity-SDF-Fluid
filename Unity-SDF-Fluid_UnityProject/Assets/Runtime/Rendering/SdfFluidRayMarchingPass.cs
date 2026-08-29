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
            
        private Material _material;
        private GraphicsBuffer _particleBuffer;
        private int _particleCount;
        private float _smoothWidth;
        #endregion
        
        #region methods

        public void Setup(Material material, GraphicsBuffer particleBuffer, int particleCount, float smoothWidth)
        {
            _material = material;
            _particleBuffer = particleBuffer;
            _particleCount = particleCount;
            _smoothWidth = smoothWidth;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            using var builder = renderGraph.AddRasterRenderPass<PassData>(RayMarchingPassName, out var passData);
            passData.Material = _material;
            passData.ParticleBuffer = renderGraph.ImportBuffer(_particleBuffer);
            passData.ParticleCount = _particleCount;
            passData.SmoothWidth = _smoothWidth;
            
            builder.UseBuffer(passData.ParticleBuffer, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc<PassData>(RenderFunc);
        }

        private static void RenderFunc(PassData passData, RasterGraphContext context)
        {
            passData.Material.SetBuffer(ParticleBufferId, passData.ParticleBuffer);
            passData.Material.SetInt(ParticleCountId, passData.ParticleCount);
            passData.Material.SetFloat(SmoothWidthId, passData.SmoothWidth);
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
            #endregion
        }
        #endregion
    }
}
