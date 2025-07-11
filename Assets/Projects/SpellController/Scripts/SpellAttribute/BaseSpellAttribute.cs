using UnityEngine;

public abstract class BaseSpellAttribute : MonoBehaviour
{
    [Header("Spell Object")]
    public Spell spell;
    public GameObject spellPrefab;
    public ParticleSystem onHitEffect;
    public AudioClip spellSound;
    public Sprite spellIcon;


    [Header("Spell Attributes")]
    public float spellCost;
    public float spellCooldown;

    [Header("Spell Caster")]
    protected GameObject spellCaster;
    protected Vector3 CasterPosition
    {
        get { return spellCaster != null ? spellCaster.transform.position : Vector3.zero; }
    }

    protected PlayerStatus PlayerStatus
    {
        get { return spellCaster.TryGetComponent(out PlayerStatus playerStatus) ? playerStatus : null; }
    }

    public bool CanCastSpell(float currentMana)
    {
        return currentMana >= spellCost;
    }


    public GameObject CastSpell(GameObject spellCaster)
    {
        if (spellCaster == null)
        {
            Debug.LogError("Spell caster is null.");
            return null;
        }


        this.spellCaster = spellCaster;

        if (spellSound != null)
        {
            if (spellCaster.TryGetComponent<AudioSource>(out var audioSource))
            {
                audioSource.PlayOneShot(spellSound);
            }
        }

        return TriggerSpell();
    }

    protected abstract GameObject TriggerSpell();

    public virtual void CopyFrom(BaseSpellAttribute other)
    {
        if (other == null) return;

        spell = other.spell;
        spellPrefab = other.spellPrefab;
        spellSound = other.spellSound;
        spellIcon = other.spellIcon;
        spellCost = other.spellCost;
        spellCooldown = other.spellCooldown;
        spellCaster = other.spellCaster;

    }
}