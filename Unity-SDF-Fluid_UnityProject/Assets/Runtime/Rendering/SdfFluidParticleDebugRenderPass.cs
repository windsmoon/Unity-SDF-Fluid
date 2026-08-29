using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Windsmoon.SdfFluid.Rendering
{
        internal class SdfFluidParticleDebugPass : ScriptableRenderPass
        {
            #region fields
            private const string ParticleDebugPassName = "SDF Fluid Particle Buffer Debug";
            private static readonly int ParticleBufferId = Shader.PropertyToID("_ParticleBuffer");
            
            private Material _material;
            private GraphicsBuffer _particleBuffer;
            private int _particleCount;
            #endregion

            #region methods
            public void Setup(Material material, GraphicsBuffer particleBuffer, int particleCount)
            {
                _material = material;
                _particleBuffer = particleBuffer;
                _particleCount = particleCount;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                using var builder = renderGraph.AddRasterRenderPass<PassData>(ParticleDebugPassName, out var passData);
                passData.Material = _material;
                passData.ParticleBuffer = renderGraph.ImportBuffer(_particleBuffer);
                passData.ParticleCount = _particleCount;

                builder.UseBuffer(passData.ParticleBuffer, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    data.Material.SetBuffer(ParticleBufferId, data.ParticleBuffer);
                    context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 6, data.ParticleCount);
                });
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