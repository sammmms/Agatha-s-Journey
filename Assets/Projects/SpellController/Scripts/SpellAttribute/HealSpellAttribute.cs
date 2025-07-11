using UnityEngine;

public class HealSpellAttribute : AuraSpellAttribute
{
    public float healAmount;


    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyHealBuff(healAmount);

        return InstantiateSpell();
    }

    public override void CancelSpell()
    {
        PlayerStatus.RemoveHealBuff(healAmount);
    }
}