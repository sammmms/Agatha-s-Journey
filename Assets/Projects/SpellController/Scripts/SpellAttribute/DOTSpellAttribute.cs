using UnityEngine;

public class DamageOverTimeSpellAttribute : BaseSpellAttribute
{
    public float dotDamage;
    public float dotDuration;
    public float dotTick;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        throw new System.NotImplementedException();
    }
}





