using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerLocomotionMapActions
{
    private bool _SprintToggled = false;
    public bool SprintToggled
    {
        get
        {
            return _SprintToggled;
        }
    }

    private bool _JumpPressed = false;
    public bool JumpPressed
    {
        get
        {
            return _JumpPressed;
        }
    }
    private bool _ManaCharged = false;
    public bool ManaCharged
    {
        get
        {
            return _ManaCharged;
        }
    }
    public PlayerControls PlayerControls { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public event Action OnDashTriggered;

    private void OnEnable()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.PlayerLocomotionMap.Enable();
        PlayerControls.PlayerLocomotionMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        PlayerControls.PlayerLocomotionMap.Disable();
        PlayerControls.PlayerLocomotionMap.RemoveCallbacks(this);
    }

    private void LateUpdate()
    {
        _JumpPressed = false;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _SprintToggled = true;
        }
        else
        {
            _SprintToggled = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        _JumpPressed = true;
    }

    public void OnZoom(InputAction.CallbackContext context)
    {

    }

    public void OnChargeMana(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _ManaCharged = true;
        }
        else
        {
            _ManaCharged = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnDashTriggered?.Invoke();
        }
    }
}
