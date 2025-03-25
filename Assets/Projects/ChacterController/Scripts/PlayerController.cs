using System.Runtime.CompilerServices;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private Camera _playerCamera;

    [Header("Player Controls")]
    public float runAcceleration = 35f;
    public float runSpeed = 4f;
    public float sprintAcceleration = 50f;
    public float sprintSpeed = 8f;
    public float drag = 20f;
    public float movingThreshold = 0.01f;

    [Header("Camera")]
    [SerializeField]
    public float lookSenseH = 0.1f;

    [SerializeField]
    public float lookSenseV = 0.1f;

    [SerializeField]
    public float lookLimitV = 89f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateMovementState();
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggled && isMovingLaterally;

        PlayerMovementState lateralState =
            isSprinting ? PlayerMovementState.Sprinting
            : isMovingLaterally || isMovementInput ? PlayerMovementState.Running
            : PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);
    }

    private void HandleLateralMovement()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        float currentAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
        float currentSpeed = isSprinting ? sprintSpeed : runSpeed;

        Vector3 cameraForwardXZ = new Vector3(
            _playerCamera.transform.forward.x,
            0,
            _playerCamera.transform.forward.z
        ).normalized;

        Vector3 cameraRightXZ = new Vector3(
            _playerCamera.transform.right.x,
            0,
            _playerCamera.transform.right.z
        ).normalized;

        Vector3 movementDirection =
            cameraRightXZ * _playerLocomotionInput.MovementInput.x
            + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        Vector3 movementDelta = movementDirection * currentAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;

        if (newVelocity.magnitude > drag * Time.deltaTime)
        {
            newVelocity = newVelocity - currentDrag;
        }
        else
        {
            newVelocity = Vector3.zero;
        }

        newVelocity = Vector3.ClampMagnitude(newVelocity, currentSpeed);

        _characterController.Move(newVelocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(
            _cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y,
            -lookLimitV,
            lookLimitV
        );

        _playerTargetRotation.x +=
            transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;
        transform.rotation = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);

        _playerCamera.transform.rotation = Quaternion.Euler(
            _cameraRotation.y,
            _cameraRotation.x,
            0
        );
    }

    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(
            _characterController.velocity.x,
            0,
            _characterController.velocity.y
        );
        return lateralVelocity.magnitude > movingThreshold;
    }
}
