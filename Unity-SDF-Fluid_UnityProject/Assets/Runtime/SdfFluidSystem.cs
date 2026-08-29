using System;
using System.Collections.Generic;
using UnityEngine;

namespace Windsmoon.SdfFluid
{
    public class SdfFluidSystem : IDisposable
    {
        #region fields
        private int _particleBufferId = Shader.PropertyToID("_ParticleBuffer");
        private int _particleCountId = Shader.PropertyToID("_ParticleCount");

        private Material _material;
        private GraphicsBuffer _particleBuffer;
        #endregion

        #region properties
        public static SdfFluidSystem ActiveSystem { get; private set; }
        public GraphicsBuffer ParticleBuffer => _particleBuffer;
        public int ParticleCount { get; private set; }
        #endregion

        #region constructors
        public SdfFluidSystem(Material material, int capacity)
        {
            _material = material;
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, FluidParticleData.Stride);
            ActiveSystem = this;
        }
        #endregion
        
        #region methods
        public void SetBuffer(List<FluidParticleData> fluidParticleDataList)
        {
            if (fluidParticleDataList == null || fluidParticleDataList.Count == 0)
            {
                ParticleCount = 0;
                _material.SetInt(_particleCountId, 0);
                return;
            }
            
            ParticleCount = fluidParticleDataList.Count;
            _particleBuffer.SetData(fluidParticleDataList, 0, 0, fluidParticleDataList.Count);
        }
        
        public void Dispose()
        {
            if (ActiveSystem == this)
            {
                ActiveSystem = null;
            }

            _particleBuffer.Dispose();
        }
        #endregion
    }
}
