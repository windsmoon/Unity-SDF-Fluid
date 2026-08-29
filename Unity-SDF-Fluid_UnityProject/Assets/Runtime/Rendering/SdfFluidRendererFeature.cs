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
        private Shader _rayMarchingShader;
        [SerializeField]
        private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        [SerializeField]
        private bool _isParticleDebugMode = false;
        [SerializeField]
        [Range(0.001f, 2.0f)]
        private float _smoothWidth = 0.2f;
        [SerializeField]
        [Range(1, 128)]
        private int _maxSteps = 40;
        [SerializeField]
        [Range(0.1f, 1.0f)]
        private float _stepSafety = 0.7f;
        [SerializeField]
        [Range(0.0001f, 0.1f)]
        private float _minStep = 0.001f;
        [SerializeField]
        [Range(0.0001f, 0.1f)]
        private float _hitEpsilon = 0.005f;
        [SerializeField]
        private Color _baseColor = new Color(0.05f, 0.35f, 0.8f, 1.0f);
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float _ambientIntensity = 0.15f;

        private Material _particleDebugMaterial;
        private Material _rayMarchingMaterial;
        
        private SdfFluidParticleDebugPass _sdfFluidParticleDebugPass;
        private SdfFluidRayMarchingPass _sdfFluidRayMarchingPass;
        #endregion

        #region methods
        public override void Create()
        {
            CoreUtils.Destroy(_particleDebugMaterial);
            CoreUtils.Destroy(_rayMarchingMaterial);
            
            _particleDebugMaterial = _particleDebugShader == null ? null : CoreUtils.CreateEngineMaterial(_particleDebugShader);
            _rayMarchingMaterial = _rayMarchingShader == null ? null : CoreUtils.CreateEngineMaterial(_rayMarchingShader);

            _sdfFluidParticleDebugPass = new SdfFluidParticleDebugPass
            {
                renderPassEvent = _renderPassEvent,
            };
            _sdfFluidRayMarchingPass = new SdfFluidRayMarchingPass()
            {
                renderPassEvent = _renderPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                return;
            }
            
            SdfFluidSystem activeSystem = SdfFluidSystem.ActiveSystem;
            if (activeSystem == null || activeSystem.ParticleCount == 0)
            {
                return;
            }

            if (_isParticleDebugMode)
            {
                AddParticleDebugPass(renderer, ref renderingData, activeSystem);
            }
            else
            {
                AddRayMarchingPass(renderer, ref renderingData, activeSystem);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_particleDebugMaterial);
            CoreUtils.Destroy(_rayMarchingMaterial);
        }

        private void AddParticleDebugPass(ScriptableRenderer renderer, ref RenderingData renderingData, SdfFluidSystem activeSystem)
        {
            if (_particleDebugMaterial == null)
            {
                Debug.LogError($"no particle debug material found on {this.name}");
                return;
            }
            
            _sdfFluidParticleDebugPass.Setup(_particleDebugMaterial, activeSystem.ParticleBuffer, activeSystem.ParticleCount);
            renderer.EnqueuePass(_sdfFluidParticleDebugPass);
        }
        
        private void AddRayMarchingPass(ScriptableRenderer renderer, ref RenderingData renderingData, SdfFluidSystem activeSystem)
        {
            if (_rayMarchingMaterial == null)
            {
                Debug.LogError($"no ray marching material found on {this.name}");
                return;
            }
            
            _sdfFluidRayMarchingPass.Setup(
                _rayMarchingMaterial,
                activeSystem.ParticleBuffer,
                activeSystem.ParticleCount,
                _smoothWidth,
                _maxSteps,
                _stepSafety,
                _minStep,
                _hitEpsilon,
                _baseColor,
                _ambientIntensity);
            renderer.EnqueuePass(_sdfFluidRayMarchingPass);
        }
        #endregion
    }
}
