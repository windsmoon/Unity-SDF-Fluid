using System.Collections.Generic;
using UnityEngine;

namespace Windsmoon.SdfFluid.ParticleProvider
{
    public interface IParticleProvider
    {
        #region methods
        public void FillParticleDataList(List<FluidParticleData> fluidParticleDataList, bool needClear = false);
        #endregion
    }
}