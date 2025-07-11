using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AttackType { Melee, Ranged }
public class EnemyController : MonoBehaviour
{
    EnemyStatus _enemyStatus;
    EnemyAnimation _enemyAnimation;
    EnemyState _enemyState;
    Rigidbody _rb;
    AudioSource _audioSource;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject _hitEffectPrefab; // Prefab for hit effect
    [SerializeField] private GameObject _deadEffectPrefab; // Prefab for death effect
    [SerializeField] private GameObject _dashTrailPrefab; // Prefab for dash trail effect
    [SerializeField] private GameObject _stunEffectPrefab; // Prefab for stun effect
    [SerializeField] private AudioClip _attackSound; // Sound to play on attack
    [SerializeField] private AudioClip _dashSound; // Sound to play on dash
    [SerializeField] private AudioClip _deathSound; // Sound to play on death
    [SerializeField] private AudioClip _stunSound; // Sound to play on stun
    [SerializeField] private List<GameObject> _rangedAttackPrefabs = new List<GameObject>(); // List of ranged attack prefabs

    [Header("Attack Pattern Settings")]
    private AttackType _currentAttackType;
    private int _attacksInCurrentString;
    private int _maxAttacksForString;

    private Coroutine _stunCoroutine;
    private void Start()
    {
        _enemyStatus = GetComponent<EnemyStatus>();
        _enemyAnimation = GetComponent<EnemyAnimation>();
        _enemyState = GetComponent<EnemyState>();
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();

        InitializeAttackPattern();
    }

    private void Update()
    {
        // Add the stun check here.
        if (_enemyStatus.IsDead || _enemyStatus.isStunned) return;

        // In Update, we decide what the enemy should be doing.
        HandleTargetting();
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f) return; // stop during pause
        if (_enemyStatus.IsDead) return;

        // If you've frozen rotation in the Rigidbody's constraints,
        // the rotation Lerp below is no longer necessary to prevent flopping over.
        // However, it can still provide a "snap back" effect if other things rotate it.
        // It's generally safe to remove if you use constraints.
        if (_enemyState.InGroundedState())
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0, transform.eulerAngles.y, 0),
                Time.fixedDeltaTime * 10f);
        }
    }


    public void TakeDamage(float damage)
    {
        _enemyStatus.TakeDamage(damage);
        if (_enemyStatus.IsDead)
        {
            Die();
        }
    }

    // Replace your old Stun and EndStun methods with these.

    public void Stun(float duration)
    {
        if (_enemyStatus.IsDead) return;

        // If the enemy is already stunned, stop the old stun routine.
        // This allows the stun duration to be "refreshed" if hit again.
        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
        }

        // Start the new stun sequence.
        _stunCoroutine = StartCoroutine(StunSequence(duration));
    }

    private IEnumerator StunSequence(float duration)
    {
        // --- 1. Begin Stun ---
        _enemyStatus.isStunned = true;
        _enemyState.SetEnemyMovementState(EnemyMovementState.Stunned);
        _enemyAnimation.PlayStunnedAnimation();

        // Forcefully interrupt any other actions.
        CancelInvoke(); // This stops pending actions like TriggerDashEnd.
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;

        // Play stun effects.
        if (_stunEffectPrefab != null)
        {
            // Parent the effect to the enemy so it moves with it.
            _stunEffectPrefab.SetActive(true);
        }
        if (_audioSource != null && _stunSound != null)
        {
            _audioSource.PlayOneShot(_stunSound);
        }


        // --- 2. Wait for Duration ---
        yield return new WaitForSeconds(duration);


        // --- 3. End Stun ---
        // Stop the visual effect.
        if (_stunEffectPrefab != null)
        {
            _stunEffectPrefab.SetActive(false);
        }

        // Make sure the enemy didn't die while stunned.
        if (_enemyStatus.IsDead) yield break;

        // Reset state.
        _enemyStatus.isStunned = false;
        _rb.isKinematic = false;
        _enemyState.SetEnemyMovementState(EnemyMovementState.Chasing);
        _enemyAnimation.EndStunnedAnimation();

        _stunCoroutine = null; // Clear the coroutine reference.
    }

    private void Die()
    {
        _rb.isKinematic = true; // Stop physics interactions
        GetComponent<Collider>().enabled = false; // Prevent further collisions
        _enemyAnimation.PlayDieAnimation(); // Play death animation
        Destroy(gameObject, 2f); // Increased delay to allow death animation
        if (_deadEffectPrefab != null)
        {
            _deadEffectPrefab.SetActive(true);
        }
        if (_audioSource != null && _deathSound != null)
        {
            _audioSource.PlayOneShot(_deathSound);
        }
    }

    // Replace the existing HandleTargetting method
    private void HandleTargetting()
    {
        GameObject target = FindClosestPlayer();

        if (target == null)
        {
            if (!_enemyState.InAttackingState())
            {
                _rb.linearVelocity = Vector3.zero;
                _enemyState.SetEnemyMovementState(EnemyMovementState.Idling);
            }
            return;
        }

        _enemyStatus.detectionRange += 15f;
        RotateTowardsTarget(target);

        bool isGrounded = _enemyState.InGroundedState();

        // --- New Attack Logic Based on Current Pattern ---
        if (_currentAttackType == AttackType.Melee && IsInMeleeRange(target) && isGrounded)
        {
            // In a MELEE string and target is in melee range -> ATTACK
            _enemyState.SetEnemyMovementState(EnemyMovementState.AttackingClose);
            _rb.linearVelocity = Vector3.zero;
            HandleMeleeAttack();
        }
        else if (_currentAttackType == AttackType.Ranged && IsInRangedAttackRange(target) && isGrounded)
        {
            // In a RANGED string and target is in ranged range -> ATTACK
            _enemyState.SetEnemyMovementState(EnemyMovementState.AttackingRanged);
            _rb.linearVelocity = Vector3.zero;
            HandleRangedAttack();
        }
        else if (IsInDashRange(target) && _enemyStatus.CanDash && isGrounded)
        {
            // Dash logic remains the same
            TriggerDash();
        }
        else if (!_enemyState.InDashingState() && !_enemyState.InAttackingState())
        {
            // If not in range for the CURRENT attack type, chase the target.
            _enemyState.SetEnemyMovementState(EnemyMovementState.Chasing);
            MoveTowardsTarget(target);
        }
    }


    private string GetAttackPattern()
    {
        // Randomize attack pattern
        return Random.Range(1, 2).ToString(); // Assuming you have 2 patterns: "1" and "2"
    }

    private void HandleMeleeAttack()
    {
        Debug.Log("Handling melee attack");
        _enemyAnimation.PlayAttackAnimation(GetAttackPattern()); // Randomize melee attack animation
    }

    private void HandleRangedAttack()
    {
        Debug.Log("Handling ranged attack");
        _enemyAnimation.PlayRangedAttackAnimation(GetAttackPattern()); // Randomize ranged attack animation
    }

    // Called when Animation is done triggering
    public void TriggerAttack()
    {

        if (_enemyStatus.IsDead) return;

        _enemyStatus.TriggerAttack();

        // Find the closest player again to ensure we have the right target
        GameObject playerObject = FindClosestPlayer();

        if (playerObject == null)
        {
            if (_enemyState.InAttackingState())
            {
                _enemyState.SetEnemyMovementState(EnemyMovementState.Chasing);
            }
            return;
        }
        ;

        bool isAttackingClose = _enemyState.CurrentEnemyMovementState == EnemyMovementState.AttackingClose;
        if (isAttackingClose)
        {
            if (_hitEffectPrefab != null)
            {
                _hitEffectPrefab.TryGetComponent<ParticleSystem>(out var hitEffect);
                if (hitEffect != null)
                {
                    hitEffect.Play();

                }
            }
        }
        bool isAttackingRanged = _enemyState.CurrentEnemyMovementState == EnemyMovementState.AttackingRanged;
        // If the enemy is in melee range, deal damage
        if (IsInMeleeRange(playerObject) && isAttackingClose)
        {
            // Play attack sound if available
            if (_audioSource != null && _attackSound != null)
            {
                _audioSource.PlayOneShot(_attackSound);
            }

            // Instantiate hit effect at the player's position


            // Deal damage to the player
            if (playerObject.TryGetComponent<PlayerController>(out var playerController))
            {
                playerController.TakeDamage(_enemyStatus.attackPower);
            }
        }

        else if (IsInRangedAttackRange(playerObject) && isAttackingRanged)
        {
            // If the enemy is in ranged attack range, instantiate a projectile
            if (_rangedAttackPrefabs.Count > 0)
            {
                GameObject projectilePrefab = _rangedAttackPrefabs[Random.Range(0, _rangedAttackPrefabs.Count)];

                if (projectilePrefab.TryGetComponent<SpellCollisionHandler>(out var projectileCollisionHandler))
                {
                    projectileCollisionHandler.enemyTag = "Player"; // Set the tag to target players
                }
                if (projectilePrefab.TryGetComponent<BaseSpellAttribute>(out var projectileAttribute))
                {
                    projectileAttribute.CastSpell(gameObject);
                }

            }
        }

        _attacksInCurrentString++;
        if (_attacksInCurrentString >= _maxAttacksForString)
        {
            SwitchAttackPattern();
        }


        if (_enemyState.InAttackingState())
        {
            _enemyState.SetEnemyMovementState(EnemyMovementState.Chasing);
        }


    }



    private void InitializeAttackPattern()
    {
        // Start with a random attack type
        _currentAttackType = (AttackType)Random.Range(0, 2);
        ResetAttackString();
    }


    private void SwitchAttackPattern()
    {
        // Flip the attack type
        _currentAttackType = (_currentAttackType == AttackType.Melee) ? AttackType.Ranged : AttackType.Melee;
        ResetAttackString();
        Debug.Log($"Enemy switching to {_currentAttackType} attack pattern for {_maxAttacksForString} attacks.");
    }


    private void ResetAttackString()
    {
        _attacksInCurrentString = 0;
        // Random.Range for integers has an exclusive max value, so 5 means the result can be 2, 3, or 4.
        _maxAttacksForString = Random.Range(1, 5);
    }

    public void TriggerDash()
    {
        if (_enemyStatus.IsDead || !_enemyStatus.CanDash) return;

        _enemyStatus.TriggerDash();

        _dashTrailPrefab.SetActive(true); // Activate dash trail effect

        _enemyState.SetEnemyMovementState(EnemyMovementState.Dashing);
        if (_audioSource != null && _dashSound != null)
        {
            _audioSource.PlayOneShot(_dashSound);
        }
        GameObject player = FindClosestPlayer();
        if (player != null)
        {
            Vector3 dashDirection = (player.transform.position - transform.position).normalized;
            float dashSpeed = _enemyStatus.dashSpeed > 0 ? _enemyStatus.dashSpeed : _enemyStatus.speed * 2f;

            // Using AddForce is generally safer for Rigidbody physics than setting velocity directly.
            _rb.linearVelocity = Vector3.zero; // Stop any current movement before applying new force
            _rb.AddForce(dashDirection * dashSpeed, ForceMode.VelocityChange);
        }

        _enemyStatus.CanAttack = false;
        Invoke(nameof(TriggerDashEnd), _enemyStatus.dashDuration);
    }

    public void TriggerDashEnd()
    {
        if (_enemyStatus.IsDead) return;

        _dashTrailPrefab.SetActive(false); // Deactivate dash trail effect
        Debug.Log("Dash ended");
        _enemyState.SetEnemyMovementState(EnemyMovementState.Chasing);
        _rb.linearVelocity = Vector3.zero; // Stop the enemy after dashing
        _enemyStatus.CanAttack = true; // Re-enable attacking after dash
    }

    private GameObject FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > _enemyStatus.detectionRange) continue;

            if (IsVisible(player))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }
        return closestPlayer;
    }

    private bool IsVisible(GameObject target)
    {
        Vector3 enemyEyes = transform.position + Vector3.up * 1f;
        Vector3 targetBody = target.transform.position + Vector3.up * 1f;
        Vector3 direction = targetBody - enemyEyes;
        float distance = direction.magnitude;

        if (Physics.Raycast(enemyEyes, direction.normalized, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return false;
            }
        }
        return true; // Nothing is between the enemy and the target
    }


    private bool IsInMeleeRange(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _enemyStatus.meleeAttackDistance) return false;

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= _enemyStatus.fieldOfView;
    }

    private bool IsInRangedAttackRange(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _enemyStatus.rangedAttackDistance) return false;

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= _enemyStatus.fieldOfView;
    }

    private bool IsInDashRange(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _enemyStatus.dashDetectionRange || distance < _enemyStatus.rangedAttackDistance) return false;

        Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= _enemyStatus.fieldOfView * 0.5f;
    }

    private void MoveTowardsTarget(GameObject target)
    {
        if (_enemyStatus.IsDead) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;


        Vector3 targetVelocity = direction * _enemyStatus.speed;
        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
    }

    private void RotateTowardsTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (_enemyStatus == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.meleeAttackDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.rangedAttackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.dashDetectionRange);
    }
}
