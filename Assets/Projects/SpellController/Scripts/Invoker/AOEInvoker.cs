using UnityEngine;

public class AOEInvoker : MonoBehaviour
{
    private EnemyTargeter _enemyTargeter;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _enemyTargeter = GetComponent<EnemyTargeter>();
    }
    public void CastAOESpell(GameObject spellPrefab)
    {
        Transform target = _enemyTargeter.FindNearestVisibleEnemy();

        // If there are no target, spawn at player position
        if (target == null)
        {
            target = _playerController.transform;
        }

        Instantiate(spellPrefab, target.position, Quaternion.identity);

    }
}