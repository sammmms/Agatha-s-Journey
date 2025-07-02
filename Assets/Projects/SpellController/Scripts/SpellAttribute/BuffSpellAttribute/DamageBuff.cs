using UnityEngine;

class DamageBuff : BuffSpellAttribute
{
    public float damageBuffAmount;
    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyDamageBuff(damageBuffAmount);

        return InstantiateSpell();
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageBuff(damageBuffAmount);
    }
}