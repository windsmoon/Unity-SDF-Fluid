using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Windsmoon.SdfFluid.Rendering
{
    public class SdfFluidRendererFeature : ScriptableRendererFeature
    {
        #region fields
        [SerializeField]
        private Shader _particleDebugShader;
        [SerializeField]
        private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        private Material _particleDebugMaterial;
        private SdfFluidParticleDebugPass _sdfFluidParticleDebugPass;
        #endregion

        #region methods
        public override void Create()
        {
            CoreUtils.Destroy(_particleDebugMaterial);
            _particleDebugMaterial = _particleDebugShader == null
                ? null
                : CoreUtils.CreateEngineMaterial(_particleDebugShader);

            _sdfFluidParticleDebugPass = new SdfFluidParticleDebugPass
            {
                renderPassEvent = _renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_particleDebugMaterial == null || renderingData.cameraData.cameraType == CameraType.Preview)
            {
                return;
            }

            SdfFluidSystem activeSystem = SdfFluidSystem.ActiveSystem;
            if (activeSystem == null || activeSystem.ParticleCount == 0)
            {
                return;
            }

            _sdfFluidParticleDebugPass.Setup(_particleDebugMaterial, activeSystem.ParticleBuffer, activeSystem.ParticleCount);
            renderer.EnqueuePass(_sdfFluidParticleDebugPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_particleDebugMaterial);
            _particleDebugMaterial = null;
            _sdfFluidParticleDebugPass = null;
        }
        #endregion
    }
}
