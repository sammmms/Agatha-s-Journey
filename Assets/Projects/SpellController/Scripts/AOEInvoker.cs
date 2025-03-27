using UnityEngine;

public class AOEInvoker : MonoBehaviour
{
    [SerializeField] private EnemyTargeter _enemyTargeter;
    public void CastAOESpell(GameObject spellPrefab)
    {
        Transform target = _enemyTargeter.FindNearestVisibleEnemy();

        // If there are no target, spawn at player position
        if (target == null)
        {
            target = transform;
        }

        Instantiate(spellPrefab, target.position, Quaternion.identity);

    }
}