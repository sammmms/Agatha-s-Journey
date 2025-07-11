using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool IsCastingSpell
    {
        get
        {
            return CurrentPlayerMovementState == PlayerMovementState.Casting ||
                CurrentPlayerMovementState == PlayerMovementState.MovingWhileCasting;
        }
        private set { }
    }
    [field: SerializeField]
    public PlayerMovementState CurrentPlayerMovementState { get; private set; } =
        PlayerMovementState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState()
    {
        return CurrentPlayerMovementState == PlayerMovementState.Idling
            || CurrentPlayerMovementState == PlayerMovementState.Walking
            || CurrentPlayerMovementState == PlayerMovementState.Running
            || CurrentPlayerMovementState == PlayerMovementState.Sprinting
            || CurrentPlayerMovementState == PlayerMovementState.Strafing
            || CurrentPlayerMovementState == PlayerMovementState.Casting
            || CurrentPlayerMovementState == PlayerMovementState.Dashing;

    }
    public bool CanCastSpell()
    {
        return InGroundedState() && CurrentPlayerMovementState != PlayerMovementState.Casting
            && CurrentPlayerMovementState != PlayerMovementState.Jumping
            && CurrentPlayerMovementState != PlayerMovementState.Falling;
    }

    public bool CanDash()
    {
        return InGroundedState() && CurrentPlayerMovementState != PlayerMovementState.Casting
            && CurrentPlayerMovementState != PlayerMovementState.Jumping
            && CurrentPlayerMovementState != PlayerMovementState.Falling
            && CurrentPlayerMovementState != PlayerMovementState.Dashing;
    }

    public bool CanJump()
    {
        return InGroundedState() && CurrentPlayerMovementState != PlayerMovementState.Casting
            && CurrentPlayerMovementState != PlayerMovementState.Jumping
            && CurrentPlayerMovementState != PlayerMovementState.Falling
            && CurrentPlayerMovementState != PlayerMovementState.Dashing;
    }
}

public enum PlayerMovementState
{
    Idling = 0,
    Walking = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Strafing = 6,
    MovingWhileCasting = 7,
    ChargingMana = 8,
    Casting = 9,
    Dashing = 10,
}
