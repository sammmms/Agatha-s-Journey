using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CharacterController _characterController;
    private PlayerStatus _playerStatusController;

    [SerializeField]
    private Camera _playerCamera;

    [Header("Player Controls")]
    public float runAcceleration = 35f;
    public float sprintAcceleration = 50f;
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

    private float _verticalVelocity = 0f;

    private float RunSpeed
    {
        get { return _playerStatusController.runSpeed; }
        set { _playerStatusController.runSpeed = value; }
    }

    private float SprintSpeed
    {
        get { return _playerStatusController.sprintSpeed; }
        set { _playerStatusController.sprintSpeed = value; }
    }

    private float JumpSpeed
    {
        get { return _playerStatusController.jumpSpeed; }
        set { _playerStatusController.jumpSpeed = value; }
    }

    private float Gravity
    {
        get { return _playerStatusController.gravity; }
        set { _playerStatusController.gravity = value; }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerStatusController = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggled && isMovingLaterally;
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState =
            isSprinting ? PlayerMovementState.Sprinting
            : isMovingLaterally || isMovementInput ? PlayerMovementState.Running
            : PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);

        if (!isGrounded && _characterController.velocity.y > 0)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        }
        else if (!isGrounded && _characterController.velocity.y <= 0)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
        }
    }
    private Vector3 _horizontalVelocity;
    private bool _wasSprintingWhenJumped;
    private void HandleVerticalMovement()
    {
        bool isGrounded = _playerState.InGroundedState();

        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = 0f;
        }

        _verticalVelocity -= Gravity * Time.deltaTime;

        if (isGrounded && _playerLocomotionInput.JumpPressed)
        {

            _wasSprintingWhenJumped = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            _verticalVelocity += Mathf.Sqrt(JumpSpeed * 3 * Gravity);


        }


        _horizontalVelocity = new Vector3(
        _characterController.velocity.x,
        0,
        _characterController.velocity.z
    );

    }

    private void HandleLateralMovement()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.InGroundedState();

        float currentAcceleration =
        isGrounded ?
        (isSprinting ? sprintAcceleration : runAcceleration) : runAcceleration * 0.3f;
        float currentSpeed =
        isSprinting ? SprintSpeed : RunSpeed;

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
            cameraRightXZ * _playerLocomotionInput.MovementInput.x +
            cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        if (isSprinting)
        {
            bool isMovingBackward = _playerLocomotionInput.MovementInput.y < 0;
            if (isMovingBackward)
            {
                currentAcceleration = runAcceleration;
                currentSpeed = RunSpeed;
                _playerState.SetPlayerMovementState(PlayerMovementState.Running);
            }
        }

        if (isGrounded)
        {
            Vector3 movementDelta = currentAcceleration * Time.deltaTime * movementDirection;
            _horizontalVelocity += movementDelta;

            Vector3 currentDrag = _horizontalVelocity.normalized * drag * Time.deltaTime;

            // Apply drag only when grounded
            if (_horizontalVelocity.magnitude > drag * Time.deltaTime)
            {
                _horizontalVelocity -= currentDrag;
            }
            else
            {
                _horizontalVelocity = Vector3.zero;
            }
        }


        Vector3 finalVelocity;
        if (!isGrounded && _wasSprintingWhenJumped)
        {
            if (_horizontalVelocity.magnitude < SprintSpeed)
            {
                _horizontalVelocity = _horizontalVelocity.normalized * SprintSpeed;
            }

            finalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, SprintSpeed);
        }
        else
        {
            finalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, currentSpeed);
        }

        finalVelocity.y = _verticalVelocity;

        _characterController.Move(finalVelocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Get smoothed look input (optional)
        Vector2 lookInput = new Vector2(
            _playerLocomotionInput.LookInput.x * lookSenseH,
            _playerLocomotionInput.LookInput.y * lookSenseV
        );

        // Horizontal rotation (Y-axis)
        _cameraRotation.x += lookInput.x;

        // Vertical rotation (X-axis) with clamping
        _cameraRotation.y = Mathf.Clamp(
            _cameraRotation.y - lookInput.y,
            -lookLimitV,
            lookLimitV
        );

        // Apply rotations using Quaternions to avoid gimbal lock
        Quaternion targetHorizontalRot = Quaternion.Euler(0f, _cameraRotation.x, 0f);
        Quaternion targetVerticalRot = Quaternion.Euler(_cameraRotation.y, 0f, 0f);

        // Player rotates only horizontally (Y-axis)
        transform.rotation = targetHorizontalRot;

        // Camera rotates vertically (X-axis) relative to player
        _playerCamera.transform.localRotation = targetVerticalRot;
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

    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }
}
