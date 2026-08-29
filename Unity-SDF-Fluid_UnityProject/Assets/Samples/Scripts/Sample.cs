using System;
using System.Collections.Generic;
using UnityEngine;
using Windsmoon.SdfFluid;
using Windsmoon.SdfFluid.ParticleProvider;

public class Sample : MonoBehaviour
{
    #region fields
    [SerializeField, Range(1, 512)] 
    private int _maxParticleCount = 64;
    [SerializeField]
    private ParticleSystemParticleProvider _particleProvider;
    [SerializeField]
    private Material _material;
    
    private SdfFluidSystem _sdfFluidSystem;
    private List<FluidParticleData> _fluidParticleDataList;
    private bool _needReinitialize;
    #endregion

    #region unity methods
    private void OnEnable()
    {
        Init();
    }


    private void OnDisable()
    {
        _sdfFluidSystem.Dispose();
        _sdfFluidSystem = null;
    }

    private void OnValidate()
    {
        _needReinitialize = true;
    }

    private void LateUpdate()
    {
        if (_needReinitialize)
        {
            Init();
        }

        _particleProvider.FillParticleDataList(_fluidParticleDataList, _maxParticleCount, true);
        _sdfFluidSystem.SetBuffer(_fluidParticleDataList);
    }
    #endregion

    #region methods
    private void Init()
    {
        _sdfFluidSystem?.Dispose();
        _sdfFluidSystem = new SdfFluidSystem(_material, _maxParticleCount);
        _fluidParticleDataList = new List<FluidParticleData>(_maxParticleCount);
        _needReinitialize = false;
    }
    #endregion
}
