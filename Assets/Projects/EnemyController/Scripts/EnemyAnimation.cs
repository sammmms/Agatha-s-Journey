using System;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _groundedAnimation = "isGrounded";
    [SerializeField] private string _chaseAnimation = "isChasing";
    [SerializeField] private string _attackAnimation = "Attack";
    [SerializeField] private string _rangedAttackAnimation = "RangedAttack";
    [SerializeField] private string _dieAnimation = "Die";

    EnemyState _enemyState;
    EnemyStatus _enemyStatus;
    private void Awake()
    {
        _enemyState = GetComponent<EnemyState>();
        _enemyStatus = GetComponent<EnemyStatus>();
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (_enemyStatus.IsDead) return;
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isGrounded = _enemyState.InGroundedState();
        bool isChasing = _enemyState.CurrentEnemyMovementState == EnemyMovementState.Chasing;
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool(_chaseAnimation, isChasing);
    }

    public void PlayAttackAnimation(string pattern = "1")
    {
        if (_enemyStatus.IsDead) return;

        Debug.Log($"Playing attack animation with pattern: {pattern}");
        _animator.SetTrigger(_attackAnimation + "_" + pattern);
    }

    public void PlayRangedAttackAnimation(string pattern = "1")
    {
        if (_enemyStatus.IsDead) return;

        Debug.Log($"Playing ranged attack animation with pattern: {pattern}");
        _animator.SetTrigger(_rangedAttackAnimation + "_" + pattern);
    }


    public void PlayDieAnimation()
    {
        _animator.SetTrigger(_dieAnimation);
    }

    public void PlayStunnedAnimation()
    {
        if (_enemyStatus.IsDead) return;

        Debug.Log("Playing stunned animation");
        _animator.SetBool("isStunned", true);
    }

    public void EndStunnedAnimation()
    {
        if (_enemyStatus.IsDead) return;

        Debug.Log("Ending stunned animation");
        _animator.SetBool("isStunned", false);
    }
}
