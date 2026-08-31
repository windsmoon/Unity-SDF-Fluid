using System.Collections.Generic;

namespace Windsmoon.SdfFluid.ParticleProvider
{
    public interface IParticleProvider
    {
        #region methods
        public void FillParticleDataList(List<FluidParticleData> fluidParticleDataList, int maxParticleCount, bool needClear = false);
        #endregion
    }
}