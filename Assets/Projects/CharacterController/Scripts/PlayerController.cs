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

    [Header("Mana Charging")]
    [SerializeField]
    public float chargeMovementSpeedMultiplier = 0.2f;
    [SerializeField]
    public float manaChargeRate = 15f; // Mana per second
    [SerializeField]
    public float chargeStaminaCost = 10f; // Stamina per second
    [SerializeField]
    public GameObject manaChargeEffectPrefab;

    public GameObject dashEffectPrefab;

    private PlayerStatus _playerStatus;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerAnimation _playerAnimation;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity = 0f;
    private bool _wasSprintingWhenJumped;
    private bool _isDashing; // Flag to indicate if a dash is active



    private Coroutine sprintCoroutine;
    private Coroutine _dashCoroutine; // To keep track of the active dash
    private Coroutine _chargeManaCoroutine; // To track the mana charge process

    #region Player Status Properties
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
    #endregion

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
        _playerAnimation = GetComponent<PlayerAnimation>();

        _playerLocomotionInput.OnDashTriggered += HandlePlayerDash;
    }

    private void Update()
    {
        HandlePlayerDeath();
        if (_playerStatus.IsDead) return;

        // Handle mana charging first as it can affect state
        HandleManaCharging();

        // The state must be updated next, as movement logic depends on it.
        UpdateMovementState();

        // If we are dashing, the coroutine handles movement, so we skip the standard handlers.

        HandleVerticalMovement();
        HandleLateralMovement();
    }

    public void TakeDamage(float damage)
    {
        if (_playerStatus.IsDead) return;

        _playerStatus.TakeDamage(damage);
        _playerAnimation.PlayHitAnimation();

        if (_playerStatus.IsDead)
        {
            _playerAnimation.PlayDeathAnimation();
            HandlePlayerDeath();
        }
    }

    private void HandlePlayerDeath()
    {
        if (_playerStatus.IsDead)
        {
            _playerLocomotionInput.enabled = false;
            _characterController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleManaCharging()
    {
        // Conditions to charge mana: input is held, player is grounded, and has stamina
        bool canCharge = _playerLocomotionInput.ManaCharged && IsGrounded() && _playerStatus.currentStaminaPoint > 0;

        if (canCharge && _chargeManaCoroutine == null)
        {
            // Start the coroutine if we can charge and it's not already running
            _playerAnimation.PlayManaChargeAnimation();
            _chargeManaCoroutine = StartCoroutine(ChargeManaCoroutine());

            if (manaChargeEffectPrefab != null)
            {
                manaChargeEffectPrefab.SetActive(true);
            }
        }
        else if (!canCharge && _chargeManaCoroutine != null)
        {
            // Stop the coroutine if the conditions are no longer met
            StopCoroutine(_chargeManaCoroutine);
            _playerAnimation.StopManaChargeAnimation();
            _chargeManaCoroutine = null;

            if (manaChargeEffectPrefab != null)
            {
                manaChargeEffectPrefab.SetActive(false);
            }
        }
    }

    private IEnumerator ChargeManaCoroutine()
    {
        while (_playerStatus.currentStaminaPoint > 0 && _playerLocomotionInput.ManaCharged)
        {
            // Recover mana over time
            float newMana = _playerStatus.currentManaPoint + manaChargeRate * Time.deltaTime;
            _playerStatus.SetMana(Mathf.Min(newMana, _playerStatus.maxManaPoint));

            // Consume stamina over time
            _playerStatus.TakeStamina(chargeStaminaCost * Time.deltaTime);

            yield return null; // Wait for the next frame
        }
        // Coroutine ends, reset the reference
        _chargeManaCoroutine = null;
    }


    // This method now checks the PlayerStatus to see if a dash is allowed.
    private void HandlePlayerDash()
    {
        // Check the CanDash property from PlayerStatus, which handles the cooldown.
        if (_playerStatus.CanDash && _dashCoroutine == null)
        {
            _dashCoroutine = StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        // --- 1. SETUP ---
        // Stamina check is still useful here as a secondary condition.
        if (_playerStatus.currentStaminaPoint < _playerStatus.dashStaminaCost)
        {
            Debug.LogWarning("Not enough stamina to dash.");
            _dashCoroutine = null;
            yield break; // Exit the coroutine
        }

        // Tell PlayerStatus to start the cooldown and consume stamina.
        _playerStatus.TriggerDash();
        _playerStatus.TakeStamina(_playerStatus.dashStaminaCost);

        // Play dash effect
        if (dashEffectPrefab != null)
        {
            dashEffectPrefab.SetActive(true);
        }

        // Set the dashing state and flag
        _isDashing = true;
        _playerState.SetPlayerMovementState(PlayerMovementState.Dashing);

        // This coroutine now only waits for the duration.
        // The actual movement is handled in the Update loop's movement handlers.
        yield return new WaitForSeconds(_playerStatus.dashDuration);

        // --- 2. CLEANUP ---
        _isDashing = false;
        _playerState.SetPlayerMovementState(PlayerMovementState.Idling); // Reset state
        _dashCoroutine = null; // Allow this controller to start a new dash coroutine.

        // Wait for around 1 second before disabling dash effect
        yield return new WaitForSeconds(1f);

        // Disable dash effect
        if (dashEffectPrefab != null)
        {
            dashEffectPrefab.SetActive(false);
        }
    }

    private void UpdateMovementState()
    {
        // If a dash is in progress, it's the only state that matters.
        if (_isDashing)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Dashing);
            return;
        }

        // If mana charging is active, it takes priority over other ground states.
        if (_chargeManaCoroutine != null)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.ChargingMana);
            return;
        }

        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool canSprint = _playerStatus.currentStaminaPoint > 0;
        bool isGrounded = IsGrounded();
        bool isMovingBackward = _playerLocomotionInput.MovementInput.y < 0;
        bool isSprinting = _playerLocomotionInput.SprintToggled && isMovingLaterally && canSprint && !isMovingBackward && isGrounded;
        bool isCasting = _playerState.IsCastingSpell;

        PlayerMovementState lateralState =
            isSprinting ? PlayerMovementState.Sprinting
            : isMovingLaterally || isMovementInput ? PlayerMovementState.Running
            : PlayerMovementState.Idling;

        if (isCasting && lateralState == PlayerMovementState.Idling)
        {
            lateralState = PlayerMovementState.Casting;
        }
        else if (isCasting)
        {
            lateralState = PlayerMovementState.MovingWhileCasting;
        }

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

    private void HandleVerticalMovement()
    {
        if (_isDashing)
        {
            // Negate gravity during the dash to allow for horizontal air dashes.
            _verticalVelocity = 0f;
            return;
        }

        // Standard gravity and jump logic for non-dashing states.
        bool isGrounded = _playerState.InGroundedState();

        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
            _wasSprintingWhenJumped = false;
        }

        if (isGrounded && _playerLocomotionInput.JumpPressed && _playerState.CanJump())
        {
            _wasSprintingWhenJumped = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            _verticalVelocity = Mathf.Sqrt(JumpSpeed * 2f * Gravity);
        }

        _verticalVelocity -= Gravity * Time.deltaTime;
    }

    private void HandleLateralMovement()
    {
        Vector3 finalVelocity;
        if (_isDashing)
        {
            // During a dash, always use dashSpeed, regardless of current velocity or buffs.
            Vector3 inputDirection = new Vector3(_playerLocomotionInput.MovementInput.x, 0, _playerLocomotionInput.MovementInput.y);
            Vector3 cameraForwardXZ = Vector3.Scale(_playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 cameraRightXZ = Vector3.Scale(_playerCamera.transform.right, new Vector3(1, 0, 1)).normalized;
            Vector3 dashDirection = inputDirection != Vector3.zero
                ? (cameraRightXZ * inputDirection.x + cameraForwardXZ * inputDirection.z).normalized
                : cameraForwardXZ;

            _horizontalVelocity = dashDirection * _playerStatus.dashSpeed;
        }
        else
        {
            // Standard lateral movement for non-dashing states.
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isCharging = _playerState.CurrentPlayerMovementState == PlayerMovementState.ChargingMana;
            bool isGrounded = _playerState.InGroundedState();

            bool maintainSprintSpeed = isSprinting || (!isGrounded && _wasSprintingWhenJumped);
            float currentSpeed = maintainSprintSpeed ? SprintSpeed : RunSpeed;

            // Slow the player down if they are charging mana
            if (isCharging)
            {
                currentSpeed *= chargeMovementSpeedMultiplier;
            }

            if (isSprinting && isGrounded)
            {
                HandleConsumeStamina();
            }

            Vector3 cameraForwardXZ = Vector3.Scale(_playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 cameraRightXZ = Vector3.Scale(_playerCamera.transform.right, new Vector3(1, 0, 1)).normalized;
            Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x +
                                         cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

            if (isCharging && movementDirection.magnitude <= 0.01f)
            {
                // If charging and no movement input, stop horizontal movement
                _horizontalVelocity = Vector3.zero;
            }
            else if (movementDirection.magnitude > 0.01f)
            {
                float currentAcceleration = isGrounded ? (maintainSprintSpeed ? sprintAcceleration : runAcceleration) : runAcceleration * 0.5f;
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, movementDirection.normalized * currentSpeed, currentAcceleration * Time.deltaTime);
            }
            else if (isGrounded)
            {
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, drag * Time.deltaTime);
            }
        }

        // The final Move call is now done once per frame for all states,
        // which is much safer for the CharacterController.
        finalVelocity = _horizontalVelocity;
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
            _playerStatus.TakeStamina(_playerStatus.sprintStaminaCost * Time.deltaTime);
            yield return new WaitForSeconds(0.05f);
            isSprinting =
                _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        }

        sprintCoroutine = null;
    }

    private void LateUpdate()
    {
        if (_playerStatus.IsDead) return;
        if (Time.timeScale == 0f) return;

        Vector2 lookInput = new(
            _playerLocomotionInput.LookInput.x * lookSenseH,
            _playerLocomotionInput.LookInput.y * lookSenseV
        );

        _cameraRotation.x += lookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookInput.y, -lookLimitV, lookLimitV);

        Quaternion targetHorizontalRot = Quaternion.Euler(0f, _cameraRotation.x, 0f);
        Quaternion targetVerticalRot = Quaternion.Euler(_cameraRotation.y, 0f, 0f);

        transform.rotation = targetHorizontalRot;
        _playerCamera.transform.localRotation = targetVerticalRot;
    }
    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new(
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
