using UnityEngine;

class SpeedBuff : BuffSpellAttribute
{
    public float speedBuffAmount;

    protected override GameObject TriggerSpell()
    {
        PlayerStatus.ApplySpeedBuff(speedBuffAmount);

        return InstantiateSpell();

    }

    public override void CancelSpell()
    {
        PlayerStatus.RemoveSpeedBuff(speedBuffAmount);
    }
}