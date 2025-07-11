using UnityEngine;

public class AreaOfEffectSpellAttribute : DamagingSpellAttribute
{
    public float impactRadius;
    public float impactStunDuration;
    public float impactStun;
    public float spellDuration;

    public AudioSource soundEffect;

    private void Start()
    {
        if (soundEffect != null)
        {
            soundEffect.Play();
        }
    }

    protected override GameObject TriggerSpell()
    {
        if (spellCaster.TryGetComponent(out AOEInvoker aoeInvoker))
        {
            aoeInvoker.CastAOESpell(spellPrefab);


            return spellPrefab;
        }
        else
        {
            Debug.LogError("AOEInvoker component not found on PlayerController.");
        }

        return null;
    }
}