using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileTracker : MonoBehaviour
{
    private Transform _target;
    private Rigidbody _rb;
    private float _speed;

    [Tooltip("How quickly the projectile can turn to face its target.")]
    [SerializeField] private float _turnSpeed = 5f;

    [Tooltip("The vertical offset from the target's pivot point to aim at (e.g., 1.0 for center mass).")]
    [SerializeField] private float _aimHeightOffset = 1.0f;

    [Tooltip("If the angle to the target is greater than this, the projectile will stop tracking.")]
    [SerializeField] private float _maxTrackingAngle = 90f; // ✨ ADD THIS

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void SetTarget(Transform target, float speed)
    {
        _target = target;
        _speed = speed;
    }

    private void FixedUpdate()
    {
        // If there's no target, continue flying straight.
        if (_target == null)
        {
            return;
        }

        Vector3 targetPoint = _target.position + Vector3.up * _aimHeightOffset;
        Vector3 directionToTarget = (targetPoint - _rb.position).normalized;

        // --- NEW LOGIC ---
        // 1. Calculate the angle between where we're facing and where the target is.
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        // 2. If the angle is too wide, stop tracking.
        if (angleToTarget > _maxTrackingAngle)
        {
            _target = null; // Setting target to null disables tracking on subsequent frames.
            return;         // Exit this frame's tracking logic.
        }
        // --- END OF NEW LOGIC ---

        // If the angle is acceptable, continue tracking as normal.
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime));
        _rb.linearVelocity = transform.forward * _speed;
    }
}