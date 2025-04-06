using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private EnemyTargeter _enemyTargeter;

    [Header("Projectile Settings")]
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _arcHeight = 2f; // Peak height of arc
    private Vector3 _destination;

    public void LaunchProjectile(GameObject prefabProjectile)
    {
        Vector3 cameraForward = _camera.transform.forward;

        Transform nearestEnemy = _enemyTargeter.FindNearestVisibleEnemy();

        if (nearestEnemy != null)
        {
            _destination = nearestEnemy.position;
        }
        else
        {
            _destination = _projectileSpawnPoint.position + cameraForward * 100f;
        }

        // Rotate player towards target
        RotatePlayerTowardsTarget();

        InstantiateObject(prefabProjectile);
    }

    private void InstantiateObject(GameObject prefab)
    {
        GameObject spawnedObject = Instantiate(prefab, _projectileSpawnPoint.position, GetProjectileRotation());

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

        Vector3 toTarget = _destination - _projectileSpawnPoint.position;

        // Calculate optimal velocity for parabolic arc
        float verticalVelocity = Mathf.Sqrt(2 * _arcHeight * Mathf.Abs(Physics.gravity.y));


        // Combine velocities
        Vector3 launchDirection = toTarget.normalized;
        launchDirection.y = verticalVelocity / (_projectileSpeed + verticalVelocity);

        rb.linearVelocity = new Vector3(
            launchDirection.x * _projectileSpeed,
            verticalVelocity,
            launchDirection.z * _projectileSpeed
        );
    }

    private Quaternion GetProjectileRotation()
    {
        // Get player's horizontal rotation (Y-axis only)
        float playerYRotation = transform.rotation.eulerAngles.y;

        return Quaternion.Euler(0, playerYRotation, 0);
    }

    private void RotatePlayerTowardsTarget()
    {
        Vector3 directionToTarget = _destination - transform.position;
        directionToTarget.y = 0; // Ignore vertical component

        print($"Direction to target: {directionToTarget}");

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
