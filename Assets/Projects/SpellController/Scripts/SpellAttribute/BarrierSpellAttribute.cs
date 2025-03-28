using UnityEngine;

public class BarrierSpellAttribute : AuraSpellAttribute
{
    public float barrierDamageReduction;

    public override GameObject castSpell(PlayerController playerController)
    {
        Vector3 position = playerController.transform.position;
        position.y += 0.8f;

        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.ApplyDamageReduction(barrierDamageReduction);

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }

    public override void cancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageReduction(barrierDamageReduction);
    }
}
