using UnityEngine;

public class AreaOfEffectSpellAttribute : BaseSpellAttribute
{
    public float impactDamage;
    public float impactRadius;
    public float impactStunDuration;
    public float impactStun;
    public float spellDuration;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        if (TryGetComponent(out AOEInvoker aoeInvoker))
        {
            aoeInvoker.CastAOESpell(spellPrefab);
        }

        return null;
    }
}