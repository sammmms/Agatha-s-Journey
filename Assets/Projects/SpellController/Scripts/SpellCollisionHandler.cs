using UnityEngine;

public class SpellCollisionHandler : MonoBehaviour
{
    ProjectileSpellAttribute _spellAttribute;

    private void Awake()
    {
        _spellAttribute = GetComponent<ProjectileSpellAttribute>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemies"))
        {
            HandleEnemyCollision(collision);
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void HandleEnemyCollision(Collision collision)
    {
        Debug.Log("Hit enemy: " + collision.gameObject.name);

        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(_spellAttribute.spellDamage); // Replace with actual damage value
        }

        // Destroy the spell object after hitting the enemy
        Destroy(gameObject);
    }
}
