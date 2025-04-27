using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _groundedAnimation = "isGrounded";
    [SerializeField] private string _walkAnimation = "isWalking";
    [SerializeField] private string _attackAnimation = "Attack";
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
        if (_enemyStatus.isDead) return;
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isGrounded = _enemyState.InGroundedState();
        bool isWalking = _enemyState.CurrentEnemyMovementState == EnemyMovementState.Walking;
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool(_walkAnimation, isWalking);
    }

    public void PlayAttackAnimation()
    {
        _animator.SetTrigger(_attackAnimation);
    }

    public void PlayDieAnimation()
    {
        _animator.SetTrigger(_dieAnimation);
    }
}
