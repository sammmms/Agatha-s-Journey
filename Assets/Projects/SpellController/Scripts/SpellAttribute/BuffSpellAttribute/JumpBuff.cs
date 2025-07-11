using UnityEngine;

class JumpBuff : BuffSpellAttribute
{
    public float jumpBuffAmount;

    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplyJumpBuff(jumpBuffAmount);

        return InstantiateSpell();
    }

    public override void CancelSpell()
    {
        PlayerStatus.RemoveJumpBuff(jumpBuffAmount);
    }
}