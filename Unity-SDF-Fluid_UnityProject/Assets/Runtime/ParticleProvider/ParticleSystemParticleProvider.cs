using System;
using System.Collections.Generic;
using UnityEngine;

namespace Windsmoon.SdfFluid.ParticleProvider
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemParticleProvider : MonoBehaviour, IParticleProvider
    {
        #region fields
        private const int ParticleStride = 32;
        
        
        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particleCache;
        private FluidParticleData[] _fluidParticleDatas;
        private GraphicsBuffer _particleBuffer;
        private int _particleCount;
        #endregion

        #region unity methods
        private void OnEnable()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            int capacity = _particleSystem.main.maxParticles;
            _particleCache = new ParticleSystem.Particle[capacity];
            _fluidParticleDatas = new FluidParticleData[capacity];
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, ParticleStride);
        }

        private void OnDisable()
        {
            _particleBuffer.Release();
            _particleBuffer = null;
        }

        private void LateUpdate()
        {
            _particleCount = _particleSystem.GetParticles(_particleCache);

            for (int i = 0; i < _particleCount; i++)
            {
                var particle = _particleCache[i];
                Color color = particle.GetCurrentColor(_particleSystem);
                _fluidParticleDatas[i] = new FluidParticleData()
                {
                    Position = particle.position,
                    Radius = particle.GetCurrentSize(_particleSystem) * 0.5f,
                    Color = new Vector4(color.r, color.g, color.b, color.a),
                };
            }

            if (_particleCount > 0)
            {
                _particleBuffer.SetData(_fluidParticleDatas, 0, 0, _particleCount);
            }
        }

        #endregion
        
        #region methods
        public void FillParticleDataList(List<FluidParticleData> fluidParticleDataList)
        {
        }
        #endregion
    }
}