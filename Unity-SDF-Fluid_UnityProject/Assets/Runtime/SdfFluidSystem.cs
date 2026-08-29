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

        #region constructors
        public SdfFluidSystem(Material material, int capacity)
        {
            _material = material;
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, 32);
        }
        #endregion
        
        #region methods
        public void SetBuffer(List<FluidParticleData> fluidParticleDataList)
        {
            if (fluidParticleDataList == null || fluidParticleDataList.Count == 0)
            {
                return;
            }
            
            _particleBuffer.SetData(fluidParticleDataList, 0, 0, fluidParticleDataList.Count);
            _material.SetBuffer(_particleBufferId, _particleBuffer);
            _material.SetInt(_particleCountId, fluidParticleDataList.Count);
        }
        
        public void Dispose()
        {
            _particleBuffer.Dispose();
        }
        #endregion
    }
}