using UnityEngine;

public class HealSpellAttribute : AuraSpellAttribute
{
    public float healAmount;


    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyHealBuff(healAmount);

        return Instantiate(spellPrefab, CasterPosition, Quaternion.identity);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus.RemoveHealBuff(healAmount);
    }
}