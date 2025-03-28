using UnityEngine;

class SpeedBuff : BuffSpellAttribute
{
    public float speedBuffAmount;

    public override GameObject castSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplySpeedBuff(speedBuffAmount);

        Vector3 position = playerController.transform.position;
        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);

    }

    public override void cancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveSpeedBuff(speedBuffAmount);
    }
}