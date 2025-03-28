using UnityEngine;

class DamageBuff : BuffSpellAttribute
{
    public float damageBuffAmount;
    public override GameObject castSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyDamageBuff(damageBuffAmount);

        Vector3 position = playerController.transform.position;
        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }

    public override void cancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveDamageBuff(damageBuffAmount);
    }
}