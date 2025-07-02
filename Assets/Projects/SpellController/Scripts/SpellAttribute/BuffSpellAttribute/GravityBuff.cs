using UnityEngine;

class GravityBuff : BuffSpellAttribute
{
    public float gravityBuffAmount;

    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyGravityBuff(gravityBuffAmount);

        return InstantiateSpell();
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveGravityBuff(gravityBuffAmount);
    }
}