using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleRainEmitter : MonoBehaviour
{
    #region fields
    [SerializeField, Range(32, 1024)]
    private int _particlesPerBurst = 512;
    [SerializeField, Min(0.25f)]
    private float _burstInterval = 1.45f;
    [SerializeField, Min(0.0f)]
    private float _initialDelay = 0.35f;
    [SerializeField]
    private Vector2 _rainArea = new Vector2(4.4f, 1.5f);
    [SerializeField, Range(0.0f, 0.25f)]
    private float _spawnJitter = 0.035f;
    [SerializeField]
    private Vector2 _fallSpeed = new Vector2(0.9f, 1.25f);
    [SerializeField, Range(0.0f, 0.5f)]
    private float _lateralSpeed = 0.08f;
    [SerializeField]
    private int _randomSeed = 6006;

    private ParticleSystem _particleSystem;
    private System.Random _random;
    private float _nextBurstTime;
    #endregion

    #region unity methods
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        ParticleSystem.MainModule main = _particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = false;
    }

    private void OnEnable()
    {
        _random = new System.Random(_randomSeed);
        _nextBurstTime = Time.time + _initialDelay;

        if (_particleSystem.isPlaying == false)
        {
            _particleSystem.Play();
        }
    }

    private void Update()
    {
        if (Time.time < _nextBurstTime)
        {
            return;
        }

        EmitBurst();
        _nextBurstTime = Time.time + _burstInterval;
    }
    #endregion

    #region methods
    private void EmitBurst()
    {
        float aspectRatio = _rainArea.x / Mathf.Max(_rainArea.y, 0.01f);
        int columnCount = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(_particlesPerBurst * aspectRatio)));
        int rowCount = Mathf.Max(1, Mathf.CeilToInt((float)_particlesPerBurst / columnCount));

        for (int i = 0; i < _particlesPerBurst; i++)
        {
            int column = i % columnCount;
            int row = i / columnCount;

            // A jittered grid reads as one broad sheet while avoiding an artificial lattice after impact.
            float normalizedX = (column + 0.5f) / columnCount - 0.5f;
            float normalizedZ = (row + 0.5f) / rowCount - 0.5f;
            Vector3 localPosition = new Vector3(
                normalizedX * _rainArea.x + NextSignedFloat() * _spawnJitter,
                NextSignedFloat() * _spawnJitter,
                normalizedZ * _rainArea.y + NextSignedFloat() * _spawnJitter);

            float fallSpeed = Mathf.Lerp(_fallSpeed.x, _fallSpeed.y, NextFloat());
            Vector3 velocity = new Vector3(
                NextSignedFloat() * _lateralSpeed,
                -fallSpeed,
                NextSignedFloat() * _lateralSpeed);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = transform.TransformPoint(localPosition),
                velocity = velocity,
                applyShapeToPosition = false,
            };
            _particleSystem.Emit(emitParams, 1);
        }
    }

    private float NextFloat()
    {
        return (float)_random.NextDouble();
    }

    private float NextSignedFloat()
    {
        return NextFloat() * 2.0f - 1.0f;
    }
    #endregion
}
