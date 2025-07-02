using UnityEngine;

public class AreaOfEffectSpellAttribute : BaseSpellAttribute
{
    public float impactDamage;
    public float impactRadius;
    public float impactStunDuration;
    public float impactStun;
    public float spellDuration;

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