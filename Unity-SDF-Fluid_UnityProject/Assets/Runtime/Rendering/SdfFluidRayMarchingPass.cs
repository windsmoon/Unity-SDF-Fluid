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
            
        private Material _material;
        private GraphicsBuffer _particleBuffer;
        private int _particleCount;
        #endregion
        
        #region methods

        public void Setup(Material material)
        {
            _material = material;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            using var builder = renderGraph.AddRasterRenderPass<PassData>(RayMarchingPassName, out var passData);
            passData.Material = _material;
            
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
            builder.SetRenderFunc<PassData>(RenderFunc);
        }

        private static void RenderFunc(PassData passData, RasterGraphContext context)
        {
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
            #endregion
        }
        #endregion
    }
}