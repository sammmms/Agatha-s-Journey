using UnityEngine;

public class BuffSpellAttribute : BaseSpellAttribute
{
    public float buffDamage;
    public float buffSpeed;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        return Instantiate(spellPrefab, transform.position, Quaternion.identity);
    }
}