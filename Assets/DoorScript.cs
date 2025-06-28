using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 90f; // Degrees per second
    [SerializeField] private float _targetAngle = 90f; // Total rotation amount
    [SerializeField] private Vector3 _rotationAxis = Vector3.up; // Rotate around Y-axis

    private bool _shouldRotate = false;
    private float _currentRotation = 0f;
    private Quaternion _initialRotation;

    private void Start()
    {
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (_shouldRotate && _currentRotation < _targetAngle)
        {
            float rotationStep = _rotationSpeed * Time.deltaTime;
            transform.Rotate(_rotationAxis, rotationStep);
            _currentRotation += rotationStep;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _shouldRotate = true;
        }
    }

    // Optional: Reset door after delay
    private IEnumerator ResetDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.rotation = _initialRotation;
        _currentRotation = 0f;
        _shouldRotate = false;
    }
}

