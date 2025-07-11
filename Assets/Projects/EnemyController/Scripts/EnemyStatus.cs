using System.Collections;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Enemy Status")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth = 100f;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    [SerializeField] public float speed = 5f;
    [SerializeField] public float dashSpeed = 10f;
    [SerializeField] public float dashDuration = 1f;
    [SerializeField] public float dashCooldown = 5f; // Cooldown before the enemy can dash again

    [Header("Enemy Attack Settings")]
    [SerializeField] public float attackPower = 10f;
    [SerializeField] public float meleeAttackDistance = 1.5f;
    [SerializeField] public float rangedAttackDistance = 10f;
    [SerializeField] public float dashDetectionRange = 15f;
    [SerializeField] public float detectionRange = 20f;
    [SerializeField] public float fieldOfView = 60f;
    [SerializeField] private float _attackCooldown = 2f;

    [Header("Enemy Regen Rate")]
    [SerializeField] private float _healthRegenRate = 1f;
    [SerializeField] private float _regenCooldown = 5f; // Cooldown before regeneration starts

    private bool _isDead = false;
    public bool IsDead => _isDead;
    public bool isStunned = false;
    private bool CanAct => !_isDead && !isStunned;



    private bool _isAttacking = false;
    [SerializeField]
    public bool CanAttack
    {
        get => CanAct && !_isAttacking;
        set => _isAttacking = value;
    }

    private bool _isDashing = false;

    private bool _dashOnCooldown = false;
    [SerializeField]
    public bool CanDash => CanAct && !_isDashing && !_dashOnCooldown;
    #region Coroutine Variables
    private Coroutine _regenCoroutine;

    #endregion

    private void Start()
    {
        _currentHealth = _maxHealth;
        // Start the regeneration process
        InitiateRegen();
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            _isDead = true;
            // Handle death (e.g., play animation, drop loot, etc.)
        }
        else
        {
            // If the enemy is not dead, start the regeneration cooldown
            if (_regenCoroutine != null)
            {
                StopCoroutine(_regenCoroutine);
            }

            _regenCoroutine = StartCoroutine(RegenCooldown());
        }
    }



    private IEnumerator RegenCooldown()
    {
        yield return new WaitForSeconds(_regenCooldown); // Adjust the wait time as needed

        InitiateRegen(); // Start regeneration after cooldown
    }


    public void TriggerAttack()
    {
        if (_isDead || _isAttacking) return;

        _isAttacking = true;
        // Handle attack logic (e.g., play animation, deal damage, etc.)
        StartCoroutine(AttackCooldown());
    }

    public void TriggerDash()
    {
        if (_isDead || _isDashing) return;

        StartCoroutine(DashCoroutine());
        // Play dash animation
    }


    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);

        _isAttacking = false;
    }

    private IEnumerator DashCoroutine()
    {
        if (_isDead || _isDashing) yield break;

        _isDashing = true;

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
        _dashOnCooldown = true;

        // Start cooldown after dashing
        yield return new WaitForSeconds(dashCooldown);

        _dashOnCooldown = false;
    }

    private void InitiateRegen()
    {
        _regenCoroutine ??= StartCoroutine(RegenStatus());
    }

    private IEnumerator RegenStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Adjust the wait time as needed

            if (_currentHealth < _maxHealth && !_isDead)
            {
                _currentHealth += _healthRegenRate; // Adjust the regen rate as needed
            }
        }
    }
}
