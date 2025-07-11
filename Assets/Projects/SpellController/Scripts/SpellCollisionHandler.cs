using UnityEngine;

public class SpellCollisionHandler : MonoBehaviour
{
    DamagingSpellAttribute _spellAttribute;
    public string enemyTag = "Enemies";
    public string groundTag = "Ground";

    [Header("Spell Collision Settings")]

    [SerializeField] private float destroyDelay = 0.1f; // Delay before destroying the spell 
    [SerializeField] private bool destroyOnHit = true; // Whether to destroy the spell on hit

    private void Awake()
    {
        _spellAttribute = GetComponent<DamagingSpellAttribute>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            HandleEnemyTrigger(other);
        }
        else if (other.CompareTag(groundTag))
        {
            Destroy(gameObject);
        }
    }

    private void HandleEnemyTrigger(Collider other)
    {
        if (other.TryGetComponent<EnemyController>(out var enemy))
        {
            try
            {

                enemy.TakeDamage(_spellAttribute.spellDamage);

                if (TryGetComponent<ProjectileSpellAttribute>(out var projectileSpell))
                {
                    if (projectileSpell.stunDuration > 0)
                    {
                        enemy.Stun(projectileSpell.stunDuration);
                    }
                }

                if (_spellAttribute.onHitEffect != null)
                {
                    // Instantiate the on-hit effect at the enemy's position
                    ParticleSystem effectInstance = Instantiate(_spellAttribute.onHitEffect, enemy.transform.position, Quaternion.identity);
                    effectInstance.Play();
                    Destroy(effectInstance.gameObject, effectInstance.main.duration);
                }


            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error dealing damage to enemy: {ex.Message}");
            }
        }
        else if (other.TryGetComponent<PlayerStatus>(out var player))
        {
            try
            {
                Debug.Log($"Spell hit player: {player.name}");
                player.TakeDamage(_spellAttribute.spellDamage);


            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error dealing damage to player: {ex.Message}");
            }
        }

        if (destroyOnHit)
        {
            Destroy(gameObject, destroyDelay);
        }
        // Destroy the spell object after a short delay to allow for damage processing
    }
}
