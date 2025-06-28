using UnityEngine;

public class EnemyController : MonoBehaviour
{
    EnemyStatus _enemyStatus;
    EnemyAnimation _enemyAnimation;
    EnemyState _enemyState;
    Rigidbody _rb;

    private void Start()
    {
        _enemyStatus = GetComponent<EnemyStatus>();
        _enemyAnimation = GetComponent<EnemyAnimation>();
        _enemyState = GetComponent<EnemyState>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_enemyStatus.isDead) return;

        // Handle enemy movement and attack logic here
        if (_enemyStatus.canAttack)
        {
            GameObject target = FindClosestPlayer();

            if (target == null) return;

            if (TargetInRange(target))
            {
                Attack(target);
                return;
            }

            MoveTowardsTarget(target);
        }
    }

    private void FixedUpdate()
    {
        if (_enemyStatus.isDead) return;

        if (_enemyState.InGroundedState())
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0, transform.eulerAngles.y, 0),
                Time.fixedDeltaTime * 10f);
        }
    }

    public void TakeDamage(float damage)
    {
        _enemyStatus.TakeDamage(damage);
        if (_enemyStatus.isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle enemy death (e.g., play animation, destroy object, etc.)
        Destroy(gameObject, 0.5f);
    }

    private void Attack(GameObject target)
    {
        if (_enemyStatus.isDead || !_enemyStatus.canAttack) return;
        _enemyAnimation.PlayAttackAnimation();
        // Handle attack logic (e.g., deal damage to player, etc.)

        _enemyStatus.TriggerAttack();

        PlayerStatus playerStatus = target.GetComponent<PlayerStatus>();

        if (playerStatus != null)
        {
            playerStatus.TakeDamage(_enemyStatus.attackPower);
        }
    }

    private GameObject FindClosestPlayer()
    {
        // Find using detection range
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        print("Player: " + players);

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            print("Distance: " + distance);
            if (distance > _enemyStatus.detectionRange) continue;

            bool isVisible = IsVisible(player);

            print("IsVisible: " + isVisible);
            if (distance < closestDistance && isVisible)
            {
                print("Closest player ++: " + closestPlayer?.name);
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    private bool IsVisible(GameObject target)
    {
        // Get approximate body center (adjust height as needed)
        Vector3 enemyEyes = transform.position + Vector3.up * 1f; // Enemy eye level
        Vector3 targetBody = target.transform.position + Vector3.up * 1f; // Player chest level

        Vector3 direction = targetBody - enemyEyes;
        float distance = direction.magnitude;
        direction.Normalize();

        // Visualize the raycast (editor only)

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            Debug.DrawRay(enemyEyes, direction * distance, Color.red, 1f);
            // Visualize the raycast for debugging
            if (hit.collider.gameObject == target)
            {
                return true;
            }
        }
        return false;
    }

    private bool TargetInRange(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= _enemyStatus.attackRange;
    }

    private void MoveTowardsTarget(GameObject target)
    {
        if (_enemyStatus.isDead) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * _enemyStatus.speed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (_enemyStatus == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _enemyStatus.attackRange);
    }
}
