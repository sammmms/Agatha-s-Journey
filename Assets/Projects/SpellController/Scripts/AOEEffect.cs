using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEEffect : MonoBehaviour
{
    private AreaOfEffectSpellAttribute _spellAttribute;

    [Header("Effect Settings")]
    [SerializeField] private List<string> _enemyTags = new() { "Enemy", "Player" };
    [Tooltip("How often the damage is applied to targets within the AOE (in seconds).")]
    [SerializeField] private float _damageTickInterval = 1.0f;
    [SerializeField] private ParticleSystem _particles;

    [SerializeField]
    private List<Collider> _targetsInZone = new List<Collider>();
    private List<Collider> _stunnedTargets = new List<Collider>();

    private void Start()
    {
        // --- Component Checks ---
        _spellAttribute = GetComponent<AreaOfEffectSpellAttribute>();
        if (_spellAttribute == null)
        {
            Debug.LogError("AreaOfEffectSpellAttribute component is missing on the AOEEffect GameObject.");
            Destroy(gameObject);
            return;
        }

        if (_particles == null)
        {
            try
            {
                _particles = GetComponentInChildren<ParticleSystem>();

            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get ParticleSystem component: {ex.Message}");
                return;
            }
        }

        // --- Start the main effect coroutine ---
        StartCoroutine(ApplyEffectOverTime());
    }

    private IEnumerator ApplyEffectOverTime()
    {
        Debug.Log($"Starting AOE effect with duration: {_spellAttribute.spellDuration} seconds");
        // --- 1. Start Visuals ---
        _particles.Play();
        float elapsedTime = 0f;

        // --- 2. Damage Loop ---
        // This loop runs for the entire duration of the spell.
        while (elapsedTime < _spellAttribute.spellDuration)
        {
            Debug.Log($"Applying AOE effects for {elapsedTime}/{_spellAttribute.spellDuration} seconds.");
            // Apply effects to all targets currently in the trigger zone.
            // We iterate over a copy in case the list is modified during the loop.
            foreach (var target in new List<Collider>(_targetsInZone))
            {
                Debug.Log($"Applying effects to target: {target.name}");
                ApplyEffects(target);
            }

            // Wait for the next damage tick.
            yield return new WaitForSeconds(_damageTickInterval);
            elapsedTime += _damageTickInterval;
        }

        // --- 3. Cleanup ---
        _particles.Stop();
        // Wait until all particles have disappeared.
        yield return new WaitWhile(() => _particles.IsAlive());
        Destroy(gameObject);
    }

    /// <summary>
    /// Applies damage and a one-time stun to a single target.
    /// </summary>
    private void ApplyEffects(Collider target)
    {
        Debug.Log($"Applying effects to target: {target.name}");
        if (target == null) return; // Target might have been destroyed.


        if (target.TryGetComponent(out EnemyController enemy))
        {
            enemy.TakeDamage(_spellAttribute.spellDamage);
            // Only apply stun ONCE per target, per spell cast.
            if (!_stunnedTargets.Contains(target))
            {
                enemy.Stun(_spellAttribute.impactStunDuration);
                _stunnedTargets.Add(target);
            }
        }
        else if (target.TryGetComponent(out PlayerController playerController))
        {
            Debug.Log($"Spell hit player: {playerController.name}");
            playerController.TakeDamage(_spellAttribute.spellDamage);
        }
    }


    /// <summary>
    /// When a valid target enters the trigger, add it to our list.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_enemyTags.Contains(other.tag) && !_targetsInZone.Contains(other))
        {
            _targetsInZone.Add(other);
        }
    }

    /// <summary>
    /// When a target leaves the trigger, remove it from our list.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (_enemyTags.Contains(other.tag))
        {
            _targetsInZone.Remove(other);
        }
    }
}