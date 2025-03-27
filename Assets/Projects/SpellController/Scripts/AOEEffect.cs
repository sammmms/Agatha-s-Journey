using System.Collections;
using UnityEngine;

public class AOEEffect : MonoBehaviour
{
    [SerializeField] private float _duration = 3f;
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private SphereCollider _damageZone;

    AreaOfEffectSpellAttribute _spellAttribute;

    private void Start()
    {
        _spellAttribute = GetComponent<AreaOfEffectSpellAttribute>();
        StartCoroutine(EffectLifecycle());
        Initialize();
    }

    public void Initialize()
    {
        _duration = _spellAttribute.spellDuration;
        _damageZone.radius = _spellAttribute.impactRadius;
    }

    private IEnumerator EffectLifecycle()
    {
        // Active phase
        _particles.Play();
        yield return new WaitForSeconds(_duration);

        // Cleanup phase
        _particles.Stop();
        yield return new WaitWhile(() => _particles.IsAlive());

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Apply damage/stun here
        }
    }
}
