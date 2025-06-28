using System.Linq;
using UnityEngine;

public class EnemyTargeter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _detectionRadius = 10f;
    [SerializeField] private string _enemyTag = "Enemies";
    [SerializeField] private string _obstacleTag = "Ground";
    [SerializeField] private int _rayCount = 8;

    public Transform FindNearestVisibleEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag)
            .Where(go => Vector3.Distance(transform.position, go.transform.position) <= _detectionRadius)
            .ToArray();

        if (enemies.Length == 0) return null;

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

        if (!IsObstacleBetween(transform.position, dirToEnemy, distance))
        {
            return true;
        }

        return false;
    }

    private bool IsObstacleBetween(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance);

        print("Raycast hits: " + hits.Length);

        // Visualize the ray on trigger
        Debug.DrawRay(origin, direction * distance, Color.red, 1f);

        return hits.Any(hit => hit.collider.CompareTag(_obstacleTag));
    }

    // Visualize in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}