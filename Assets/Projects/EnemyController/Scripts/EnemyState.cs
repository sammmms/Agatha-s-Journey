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
            || CurrentEnemyMovementState == EnemyMovementState.Chasing;

    }

    public bool InAttackingState()
    {
        return CurrentEnemyMovementState == EnemyMovementState.AttackingClose
            || CurrentEnemyMovementState == EnemyMovementState.AttackingRanged;
    }

    public bool InDashingState()
    {
        return CurrentEnemyMovementState == EnemyMovementState.Dashing;
    }

}

public enum EnemyMovementState
{
    Idling = 0,
    Chasing = 1,
    Dashing = 2,
    AttackingClose = 3,
    AttackingRanged = 4,
    Stunned = 5,
}