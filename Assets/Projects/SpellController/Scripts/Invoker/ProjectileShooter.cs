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

    private PlayerAnimation _playerAnimation;
    private Vector3 _destination;

    private void Start()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

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

        InstantiateObject(prefabProjectile);
    }

    private void InstantiateObject(GameObject prefab)
    {
        _playerAnimation.CastSpellAnimation();
        GameObject spawnedObject = Instantiate(prefab, _projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

        Vector3 toTarget = _destination - _projectileSpawnPoint.position;
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

        // Calculate optimal velocity for parabolic arc
        float verticalVelocity = Mathf.Sqrt(2 * _arcHeight * Mathf.Abs(Physics.gravity.y));
        float flightTime = verticalVelocity * 2 / Mathf.Abs(Physics.gravity.y);
        float horizontalVelocity = horizontalDistance / flightTime;

        // Combine velocities
        Vector3 launchDirection = toTarget.normalized;
        launchDirection.y = verticalVelocity / (horizontalVelocity + verticalVelocity);

        rb.linearVelocity = new Vector3(
            launchDirection.x * horizontalVelocity,
            verticalVelocity,
            launchDirection.z * horizontalVelocity
        );
    }
}
