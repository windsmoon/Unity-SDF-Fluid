using System;
using System.Collections.Generic;
using UnityEngine;

namespace Windsmoon.SdfFluid.ParticleProvider
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemParticleProvider : MonoBehaviour, IParticleProvider
    {
        #region fields
        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particleCache;
        private FluidParticleData[] _fluidParticleDatas;
        private int _particleCount;
        #endregion

        #region unity methods
        private void OnEnable()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            int capacity = _particleSystem.main.maxParticles;
            _particleCache = new ParticleSystem.Particle[capacity];
            _fluidParticleDatas = new FluidParticleData[capacity];
        }
        #endregion
        
        #region methods
        public void FillParticleDataList(List<FluidParticleData> fluidParticleDataList, bool needClear = false)
        {
            if (needClear)
            {
                fluidParticleDataList.Clear();
            }
            
            if (enabled == false)
            {
                return;
            }
            
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
        }
        #endregion
    }
}