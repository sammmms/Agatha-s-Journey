using UnityEngine;

class JumpBuff : BuffSpellAttribute
{
    public float jumpBuffAmount;

    public override GameObject CastSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyJumpBuff(jumpBuffAmount);

        return InstantiateSpell(playerController);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveJumpBuff(jumpBuffAmount);
    }
}