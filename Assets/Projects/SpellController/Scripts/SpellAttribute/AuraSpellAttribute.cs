using UnityEngine;

abstract public class AuraSpellAttribute : BaseSpellAttribute
{
    public abstract void CancelSpell(PlayerController playerController);

    public GameObject InstantiateSpell(PlayerController playerController)
    {
        if (spellPrefab == null) return null;

        Vector3 position = playerController.transform.position;

        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }
}