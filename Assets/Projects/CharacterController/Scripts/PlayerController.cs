using System;
using System.Collections;
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

    private PlayerStatus _playerStatus;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private Vector2 _cameraRotation = Vector2.zero;

    private float _verticalVelocity = 0f;

    private Coroutine sprintCoroutine;

    private float RunSpeed
    {
        get { return _playerStatus.runSpeed; }
        set { _playerStatus.runSpeed = value; }
    }

    private float SprintSpeed
    {
        get { return _playerStatus.sprintSpeed; }
        set { _playerStatus.sprintSpeed = value; }
    }

    private float JumpSpeed
    {
        get { return _playerStatus.jumpSpeed; }
        set { _playerStatus.jumpSpeed = value; }
    }

    private float Gravity
    {
        get { return _playerStatus.gravity; }
        set { _playerStatus.gravity = value; }
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
        _playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {

        HandlePlayerDeath();
        if (_playerStatus.isDead) return;
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void HandlePlayerDeath()
    {
        if (_playerStatus.isDead)
        {
            _playerLocomotionInput.enabled = false;
            _characterController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateMovementState()
    {
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool canSprint = _playerStatus.currentStaminaPoint > 0;
        bool isGrounded = IsGrounded();
        bool isMovingBackward = _playerLocomotionInput.MovementInput.y < 0;
        bool isSprinting = _playerLocomotionInput.SprintToggled && isMovingLaterally && canSprint && !isMovingBackward && isGrounded;

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
            _verticalVelocity = -2f;

            _wasSprintingWhenJumped = false;
        }

        if (isGrounded && _playerLocomotionInput.JumpPressed)
        {
            _wasSprintingWhenJumped = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            _verticalVelocity = Mathf.Sqrt(JumpSpeed * 2f * Gravity); // Corrected jump formula
        }

        _verticalVelocity -= Gravity * Time.deltaTime;
    }

    private void HandleLateralMovement()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.InGroundedState();

        // --- MODIFIED SPEED LOGIC ---
        // We should maintain sprint speed if we are currently sprinting OR if we are in the air
        // and the jump was initiated from a sprint.
        bool maintainSprintSpeed = isSprinting || (!isGrounded && _wasSprintingWhenJumped);
        float currentSpeed = maintainSprintSpeed ? SprintSpeed : RunSpeed;
        // --- END OF MODIFICATION ---

        if (isSprinting && isGrounded)
        {
            HandleConsumeStamina();
        }

        Vector3 cameraForwardXZ = Vector3.Scale(_playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRightXZ = Vector3.Scale(_playerCamera.transform.right, new Vector3(1, 0, 1)).normalized;
        Vector3 movementDirection = (cameraRightXZ * _playerLocomotionInput.MovementInput.x +
                                     cameraForwardXZ * _playerLocomotionInput.MovementInput.y);

        if (movementDirection.magnitude > 0.01f)
        {
            // Use a ternary for acceleration as well, makes it cleaner
            float currentAcceleration = isGrounded ? (maintainSprintSpeed ? sprintAcceleration : runAcceleration) : runAcceleration * 0.5f;

            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, movementDirection.normalized * currentSpeed, currentAcceleration * Time.deltaTime);
        }
        else if (isGrounded)
        {
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, drag * Time.deltaTime);
        }

        Vector3 finalVelocity = _horizontalVelocity;
        finalVelocity.y = _verticalVelocity;

        _characterController.Move(finalVelocity * Time.deltaTime);
    }


    private void HandleConsumeStamina()
    {
        sprintCoroutine ??= StartCoroutine(nameof(ConsumeStamina));
    }

    private IEnumerator ConsumeStamina()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        while (isSprinting)
        {
            _playerStatus.SetStamina(_playerStatus.currentStaminaPoint - 0.5f);
            yield return new WaitForSeconds(0.05f);
            isSprinting =
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        }

        sprintCoroutine = null;
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
            _characterController.velocity.z
        );
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }
}
