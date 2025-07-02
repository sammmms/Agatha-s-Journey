using UnityEngine;

class SpeedBuff : BuffSpellAttribute
{
    public float speedBuffAmount;

    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplySpeedBuff(speedBuffAmount);

        return InstantiateSpell();

    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveSpeedBuff(speedBuffAmount);
    }
}