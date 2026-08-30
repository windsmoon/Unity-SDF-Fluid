using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ParticleSystem))]
public class WaterGunEmitter : MonoBehaviour
{
    #region fields
    [SerializeField, Min(1.0f)]
    private float _particlesPerSecond = 560.0f;
    [SerializeField]
    private Vector2 _speedRange = new Vector2(10.5f, 12.5f);
    [SerializeField, Range(0.0f, 12.0f)]
    private float _spreadAngle = 1.35f;
    [SerializeField, Min(0.0f)]
    private float _spawnRadius = 0.035f;
    [SerializeField]
    private Camera _aimCamera;
    [SerializeField]
    private Transform _aimPivot;
    [SerializeField, Min(1.0f)]
    private float _aimDistance = 30.0f;
    [SerializeField]
    private int _randomSeed = 6106;

    private ParticleSystem _particleSystem;
    private System.Random _random;
    private float _emissionAccumulator;
    #endregion

    #region unity methods
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_aimCamera == null)
        {
            _aimCamera = GetComponentInParent<Camera>();
        }

        if (_aimPivot == null)
        {
            _aimPivot = transform.parent;
        }

        ParticleSystem.MainModule main = _particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = _particleSystem.emission;
        emission.enabled = false;
    }

    private void OnEnable()
    {
        _random = new System.Random(_randomSeed);
        _emissionAccumulator = 0.0f;

        if (_particleSystem.isPlaying == false)
        {
            _particleSystem.Play();
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            UpdateAim(mouse.position.ReadValue());
        }

        if (mouse == null || mouse.leftButton.isPressed == false)
        {
            _emissionAccumulator = 0.0f;
            return;
        }

        _emissionAccumulator += _particlesPerSecond * Time.deltaTime;
        int emitCount = Mathf.FloorToInt(_emissionAccumulator);
        _emissionAccumulator -= emitCount;

        for (int i = 0; i < emitCount; i++)
        {
            // Backdating each sample across this frame avoids visible frame-sized packets in the jet.
            float subFrameAge = Time.deltaTime * ((i + NextFloat()) / emitCount);
            EmitParticle(subFrameAge);
        }
    }
    #endregion

    #region methods
    private void UpdateAim(Vector2 mousePosition)
    {
        if (_aimCamera == null || _aimPivot == null)
        {
            return;
        }

        Ray mouseRay = _aimCamera.ScreenPointToRay(mousePosition);
        Vector3 targetPosition = mouseRay.GetPoint(_aimDistance);
        Vector3 aimDirection = targetPosition - _aimPivot.position;
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        // The gun converges on the cursor ray instead of merely firing parallel from its screen offset.
        _aimPivot.rotation = Quaternion.LookRotation(aimDirection.normalized, _aimCamera.transform.up);
    }

    private void EmitParticle(float subFrameAge)
    {
        float spawnDistance = Mathf.Sqrt(NextFloat()) * _spawnRadius;
        float spawnAngle = NextFloat() * Mathf.PI * 2.0f;
        Vector3 spawnOffset = transform.right * (Mathf.Cos(spawnAngle) * spawnDistance)
            + transform.up * (Mathf.Sin(spawnAngle) * spawnDistance);

        // Sampling a disk and projecting it forward produces an even cone instead of a dense center line.
        float spreadDistance = Mathf.Sqrt(NextFloat()) * Mathf.Tan(_spreadAngle * Mathf.Deg2Rad);
        float spreadRotation = NextFloat() * Mathf.PI * 2.0f;
        Vector3 direction = (transform.forward
            + transform.right * (Mathf.Cos(spreadRotation) * spreadDistance)
            + transform.up * (Mathf.Sin(spreadRotation) * spreadDistance)).normalized;

        float speed = Mathf.Lerp(_speedRange.x, _speedRange.y, NextFloat());
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = transform.position + spawnOffset + direction * speed * subFrameAge,
            velocity = direction * speed,
            applyShapeToPosition = false,
        };
        _particleSystem.Emit(emitParams, 1);
    }

    private float NextFloat()
    {
        return (float)_random.NextDouble();
    }
    #endregion
}
