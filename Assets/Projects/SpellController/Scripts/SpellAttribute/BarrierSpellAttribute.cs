using UnityEngine;

public class BarrierSpellAttribute : AuraSpellAttribute
{
    public float barrierDamageReduction;

    public override GameObject CastSpell(PlayerController playerController)
    {
        Vector3 position = playerController.transform.position;
        position.y += 0.8f;

        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.ApplyDamageReduction(barrierDamageReduction);

        return InstantiateSpell(playerController);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageReduction(barrierDamageReduction);
    }
}
