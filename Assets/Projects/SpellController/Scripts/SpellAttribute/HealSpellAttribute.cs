using UnityEngine;

public class HealSpellAttribute : AuraSpellAttribute
{
    public float healAmount;


    public override GameObject castSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyHealBuff(healAmount);

        return Instantiate(spellPrefab, playerController.transform.position, Quaternion.identity);
    }

    public override void cancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveHealBuff(healAmount);
    }
}