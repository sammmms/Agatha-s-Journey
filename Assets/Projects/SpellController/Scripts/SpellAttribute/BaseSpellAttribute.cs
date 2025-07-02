using UnityEngine;

public abstract class BaseSpellAttribute : MonoBehaviour
{
    [Header("Spell Object")]
    public Spell spell;
    public GameObject spellPrefab;
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

    public bool CanCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
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
}