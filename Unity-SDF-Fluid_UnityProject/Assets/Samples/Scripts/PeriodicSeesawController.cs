using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PeriodicSeesawController : MonoBehaviour
{
    #region fields
    [SerializeField, Range(5.0f, 35.0f)]
    private float _maxAngle = 22.0f;
    [SerializeField, Min(1.0f)]
    private float _cycleDuration = 4.8f;

    private Rigidbody _rigidbody;
    private Quaternion _baseRotation;
    #endregion

    #region unity methods
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        _baseRotation = _rigidbody.rotation;
    }

    private void FixedUpdate()
    {
        // The sine phase alternates which end is raised while crossing the level pose smoothly.
        float phase = Mathf.Repeat(Time.fixedTime / _cycleDuration, 1.0f);
        float angle = Mathf.Sin(phase * Mathf.PI * 2.0f) * _maxAngle;
        Quaternion targetRotation = _baseRotation * Quaternion.AngleAxis(angle, Vector3.forward);
        _rigidbody.MoveRotation(targetRotation);
    }
    #endregion
}
