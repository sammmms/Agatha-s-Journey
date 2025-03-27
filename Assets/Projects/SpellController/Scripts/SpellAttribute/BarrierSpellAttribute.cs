using UnityEngine;

public class BarrierSpellAttribute : BaseSpellAttribute
{
    public float barrierDamageReduction;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        Vector3 position = transform.position;
        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }
}
