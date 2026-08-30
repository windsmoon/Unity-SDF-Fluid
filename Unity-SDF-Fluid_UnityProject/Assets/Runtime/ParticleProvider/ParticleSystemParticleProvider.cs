using System;
using System.Collections.Generic;
using UnityEngine;

namespace Windsmoon.SdfFluid.ParticleProvider
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemParticleProvider : MonoBehaviour, IParticleProvider
    {
        #region fields
        [SerializeField, Range(0.0f, 2.0f)]
        private float _downwardOffsetRatio = 1f;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particleCache;
        private int _particleCount;
        #endregion

        #region unity methods
        private void OnEnable()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            int capacity = _particleSystem.main.maxParticles;
            _particleCache = new ParticleSystem.Particle[capacity];
        }
        #endregion
        
        #region methods
        public void FillParticleDataList(List<FluidParticleData> fluidParticleDataList, int maxParticleCount, bool needClear = false)
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

            for (int i = 0; i < maxParticleCount && i < _particleCount; i++)
            {
                var particle = _particleCache[i];
                Color color = particle.GetCurrentColor(_particleSystem);
                float radius = particle.GetCurrentSize(_particleSystem) * 0.5f;
                fluidParticleDataList.Add(new FluidParticleData()
                {
                    // Lower only the reconstructed SDF center. The simulated particle keeps its
                    // original position so collision and motion do not sink into the scene floor.
                    Position = particle.position + Vector3.down * (radius * _downwardOffsetRatio),
                    Radius = radius,
                    Color = new Vector4(color.r, color.g, color.b, color.a),
                });
            }
        }
        #endregion
    }
}
