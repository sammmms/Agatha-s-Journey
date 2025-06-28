using UnityEngine;

class SpeedBuff : BuffSpellAttribute
{
    public float speedBuffAmount;

    public override GameObject CastSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplySpeedBuff(speedBuffAmount);

        return InstantiateSpell(playerController);

    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveSpeedBuff(speedBuffAmount);
    }
}