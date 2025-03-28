using UnityEngine;

class JumpBuff : BuffSpellAttribute
{
    public float jumpBuffAmount;
    public float gravityBuffAmount;

    public override GameObject castSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();

        playerStatus.ApplyJumpBuff(jumpBuffAmount, gravityBuffAmount);

        Vector3 position = playerController.transform.position;
        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }

    public override void cancelSpell(PlayerController playerController)
    {
        PlayerStatus playerStatus = playerController.GetComponent<PlayerStatus>();
        playerStatus.RemoveJumpBuff(jumpBuffAmount, gravityBuffAmount);
    }
}