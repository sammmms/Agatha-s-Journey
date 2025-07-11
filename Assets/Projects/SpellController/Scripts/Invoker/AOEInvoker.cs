using System.Collections;
using UnityEngine;

public class AOEInvoker : MonoBehaviour
{
    private EnemyTargeter _enemyTargeter;
    [SerializeField] private float _delayBeforeCast = 1.5f;
    [SerializeField] private ParticleSystem _placeholderParticle; // The warning indicator prefab

    private void Awake()
    {
        _enemyTargeter = GetComponent<EnemyTargeter>();
    }

    public void CastAOESpell(GameObject spellPrefab)
    {
        Transform target = _enemyTargeter.FindNearestVisibleEnemy();
        Vector3 targetPosition;

        if (target == null)
        {
            targetPosition = transform.position;
        }
        else
        {
            targetPosition = target.position;
        }

        StartCoroutine(DelayBeforeCast(spellPrefab, targetPosition));
    }

    private IEnumerator DelayBeforeCast(GameObject spellPrefab, Vector3 position)
    {
        ParticleSystem placeholderInstance = null;
        if (_placeholderParticle != null)
        {
            placeholderInstance = Instantiate(_placeholderParticle, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Placeholder particle is not set. No visual will be shown.");
        }

        // Wait for the cast delay.
        yield return new WaitForSeconds(_delayBeforeCast);

        // Clean up by destroying the placeholder's GameObject.
        if (placeholderInstance != null)
        {
            // Make sure to destroy the GameObject, not just the component.
            Destroy(placeholderInstance.gameObject);
        }

        // Spawn the actual spell.
        InstantiateSpell(spellPrefab, position);
    }

    private void InstantiateSpell(GameObject spellPrefab, Vector3 position)
    {
        if (spellPrefab == null)
        {
            Debug.LogWarning("Spell prefab is null.");
            return;
        }

        Instantiate(spellPrefab, position, Quaternion.identity);
    }
}