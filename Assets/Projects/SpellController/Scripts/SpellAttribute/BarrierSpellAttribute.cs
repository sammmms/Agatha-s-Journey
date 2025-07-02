using UnityEngine;

public class BarrierSpellAttribute : AuraSpellAttribute
{
    public float barrierDamageReduction;

    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyDamageReduction(barrierDamageReduction);

        return InstantiateSpell();
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageReduction(barrierDamageReduction);
    }
}
