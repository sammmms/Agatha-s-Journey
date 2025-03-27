using UnityEngine;

public class HealSpellAttribute : BaseSpellAttribute
{
    public float healAmount;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        return Instantiate(spellPrefab, transform.position, Quaternion.identity);
    }
}