using System.Collections;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [Header("Enemy Status")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth = 100f;
    [SerializeField] public float speed = 5f;
    [SerializeField] public float attackPower = 10f;
    [SerializeField] public float attackRange = 2f;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] public float detectionRange = 10f;
    [SerializeField] private float _fieldOfView = 60f;
    [SerializeField] private float _fieldOfViewAngle = 45f;
    [SerializeField] private float _fieldOfViewDistance = 10f;

    [Header("Enemy Regen Rate")]
    [SerializeField] private float _healthRegenRate = 1f;

    private bool _isDead = false;
    public bool isDead => _isDead;

    private bool _isAttacking = false;
    public bool canAttack
    {
        get => !_isAttacking && !isDead;
    }

    #region Coroutine Variables
    private Coroutine _regenCoroutine;

    #endregion

    private void Start()
    {
        _currentHealth = _maxHealth;
        // Start the regeneration process
        _InitiateRegen();
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
    }

    public void TriggerAttack()
    {
        if (_isDead || _isAttacking) return;

        _isAttacking = true;
        // Handle attack logic (e.g., play animation, deal damage, etc.)
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);

        _isAttacking = false;
    }

    private void _InitiateRegen()
    {
        if (_regenCoroutine == null)
        {
            _regenCoroutine = StartCoroutine(RegenStatus());
        }
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
