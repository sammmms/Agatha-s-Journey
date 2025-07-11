using System;
using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _projectileSpawnPoint;

    private EnemyTargeter _enemyTargeter;

    private void Awake()
    {
        _enemyTargeter = GetComponent<EnemyTargeter>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    public void LaunchProjectile(GameObject prefabProjectile)
    {
        // 1. Get speed from the projectile's attribute component
        if (!prefabProjectile.TryGetComponent<ProjectileSpellAttribute>(out var projectileAttribute))
        {
            Debug.LogError("Projectile prefab is missing ProjectileSpellAttribute component!", prefabProjectile);
            return;
        }
        float spellSpeed = projectileAttribute.spellSpeed;

        // 2. Decide which launch method to use
        Transform nearestEnemy = _enemyTargeter.FindNearestVisibleEnemy();
        // --- UPDATED LOGIC ---
        // Check if the projectile should track and if there is a target.
        if (nearestEnemy != null && projectileAttribute.isTracking)
        {
            LaunchTracking(prefabProjectile, nearestEnemy, spellSpeed);
        }
        else if (nearestEnemy != null)
        {
            // Standard arc launch for non-tracking projectiles.
            LaunchArc(prefabProjectile, nearestEnemy.position, spellSpeed);
        }
        else
        {
            // Standard forward launch if no enemy is found.
            LaunchForward(prefabProjectile, spellSpeed);
        }
    }

    /// <summary>
    /// Launches a projectile and tells it to start tracking a target.
    /// </summary>
    private void LaunchTracking(GameObject prefab, Transform target, float speed)
    {
        // Aim and instantiate the projectile towards the target's initial position.
        Vector3 initialDirection = (target.position - _projectileSpawnPoint.position).normalized;
        GameObject spawnedObject = Instantiate(prefab, _projectileSpawnPoint.position, Quaternion.LookRotation(initialDirection));

        // Give it an initial velocity.
        if (spawnedObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = initialDirection * speed;
        }

        // Find the tracker component on the projectile and assign the target.
        if (spawnedObject.TryGetComponent<ProjectileTracker>(out var tracker))
        {
            tracker.SetTarget(target, speed);
        }
        else
        {
            Debug.LogWarning("This tracking projectile is missing the ProjectileTracker component!", spawnedObject);
        }
    }

    private void LaunchForward(GameObject prefab, float speed)
    {
        Vector3 aimDirection = _camera.transform.forward;

        // 2. Create a rotation that tilts the aim upwards by the desired angle
        // We rotate around the camera's horizontal (right) axis to achieve this
        Quaternion upwardRotation = Quaternion.AngleAxis(-7f, _camera.transform.right);

        // 3. Apply the rotation to get the final launch direction
        Vector3 launchDirection = upwardRotation * aimDirection;

        // Instantiate the projectile facing the new direction and set its velocity
        GameObject spawnedObject = Instantiate(prefab, _projectileSpawnPoint.position, Quaternion.LookRotation(launchDirection));
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = launchDirection * speed;
        }
    }

    /// <summary>
    /// Calculates the correct launch angle to hit a destination with a fixed speed.
    /// </summary>
    private void LaunchArc(GameObject prefab, Vector3 destination, float speed)
    {
        GameObject spawnedObject = Instantiate(prefab, _projectileSpawnPoint.position, _projectileSpawnPoint.rotation);
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 displacement = destination - _projectileSpawnPoint.position;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = displacementXZ.magnitude;
        float verticalDistance = displacement.y;

        float gravity = Physics.gravity.magnitude;

        // Formula to find the launch angle for a ballistic trajectory
        float speedSqr = speed * speed;
        float discriminant = (speedSqr * speedSqr) - gravity * (gravity * horizontalDistance * horizontalDistance + 2 * verticalDistance * speedSqr);

        if (discriminant < 0)
        {
            // Target is out of range for this speed, launch directly at it as a fallback
            Debug.LogWarning("Target is out of range. Launching directly.");
            Vector3 directDirection = (destination - _projectileSpawnPoint.position).normalized;
            rb.linearVelocity = directDirection * speed;
            return;
        }

        float launchAngle = Mathf.Atan((speedSqr - Mathf.Sqrt(discriminant)) / (gravity * horizontalDistance));
        float verticalVelocity = speed * Mathf.Sin(launchAngle);
        float horizontalVelocity = speed * Mathf.Cos(launchAngle);

        Vector3 launchDirection = displacementXZ.normalized;
        Vector3 finalVelocity = (launchDirection * horizontalVelocity) + (Vector3.up * verticalVelocity);

        rb.linearVelocity = finalVelocity;

        // Optional: Rotate the projectile to face its initial velocity direction
        if (finalVelocity != Vector3.zero)
        {
            spawnedObject.transform.rotation = Quaternion.LookRotation(finalVelocity);
        }
    }
}