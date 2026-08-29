using System.Runtime.InteropServices;
using UnityEngine;

namespace Windsmoon.SdfFluid
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FluidParticleData
    {
        #region fields
        public const int Stride = 32;

        public Vector3 Position;
        public float Radius;
        public Vector4 Color;
        #endregion
    }
}
