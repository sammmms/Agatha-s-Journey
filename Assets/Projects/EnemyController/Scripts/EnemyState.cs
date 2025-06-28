using UnityEngine;

public class EnemyState : MonoBehaviour
{
    [field: SerializeField]
    public EnemyMovementState CurrentEnemyMovementState { get; private set; } =
        EnemyMovementState.Idling;

    public void SetEnemyMovementState(EnemyMovementState enemyMovementState)
    {
        CurrentEnemyMovementState = enemyMovementState;
    }

    public bool InGroundedState()
    {
        return CurrentEnemyMovementState == EnemyMovementState.Idling
            || CurrentEnemyMovementState == EnemyMovementState.Walking
            || CurrentEnemyMovementState == EnemyMovementState.Running
            || CurrentEnemyMovementState == EnemyMovementState.Sprinting
            || CurrentEnemyMovementState == EnemyMovementState.Strafing;
    }
}

public enum EnemyMovementState
{
    Idling = 0,
    Walking = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Strafing = 6,
    Stunned = 7,
    Aiming = 8,
    Casting = 9,
}