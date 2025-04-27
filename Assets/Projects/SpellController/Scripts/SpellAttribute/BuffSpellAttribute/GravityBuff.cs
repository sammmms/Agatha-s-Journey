using UnityEngine;

class GravityBuff : BuffSpellAttribute
{
    public float gravityBuffAmount;

    public override GameObject CastSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyGravityBuff(gravityBuffAmount);

        return InstantiateSpell(playerController);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveGravityBuff(gravityBuffAmount);
    }
}