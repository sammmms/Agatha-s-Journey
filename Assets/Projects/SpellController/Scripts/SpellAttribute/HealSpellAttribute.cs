using UnityEngine;

public class HealSpellAttribute : AuraSpellAttribute
{
    public float healAmount;


    public override GameObject CastSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyHealBuff(healAmount);

        return Instantiate(spellPrefab, playerController.transform.position, Quaternion.identity);
    }

    public override void CancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveHealBuff(healAmount);
    }
}