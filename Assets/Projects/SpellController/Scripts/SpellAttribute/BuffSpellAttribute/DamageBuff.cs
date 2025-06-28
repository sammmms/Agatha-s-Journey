using UnityEngine;

class DamageBuff : BuffSpellAttribute
{
    public float damageBuffAmount;
    public override GameObject CastSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyDamageBuff(damageBuffAmount);

        return InstantiateSpell(playerController);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageBuff(damageBuffAmount);
    }
}