using System.Linq;
using UnityEngine;

public class EnemyTargeter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _detectionRadius = 10f;
    [SerializeField] private string _enemyTag = "Enemies";
    [SerializeField] private string _obstacleTag = "Ground"; // Using tag for obstacles
    [SerializeField] private int _rayCount = 8; // Rays in a circle

    public Transform FindNearestVisibleEnemy()
    {
        // 1. Find all enemies in radius using tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag)
            .Where(go => Vector3.Distance(transform.position, go.transform.position) <= _detectionRadius)
            .ToArray();

        if (enemies.Length == 0) return null;

        // 2. Filter visible enemies with raycast
        var visibleEnemies = enemies
            .Where(enemy => IsVisible(enemy.transform))
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .ToArray();

        return visibleEnemies.Length > 0 ? visibleEnemies[0].transform : null;
    }

    private bool IsVisible(Transform enemy)
    {
        Vector3 dirToEnemy = (enemy.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, enemy.position);

        // 3. Central ray (direct line of sight)
        if (!IsObstacleBetween(transform.position, dirToEnemy, distance))
        {
            return true;
        }

        // 4. Peripheral rays (for edge cases)
        for (int i = 0; i < _rayCount; i++)
        {
            float angle = i * (360f / _rayCount);
            Vector3 rayDir = Quaternion.Euler(0, angle, 0) * dirToEnemy;

            if (!IsObstacleBetween(transform.position, rayDir, distance))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsObstacleBetween(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance);
        return hits.Any(hit => hit.collider.CompareTag(_obstacleTag));
    }

    // Visualize in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}