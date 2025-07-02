using UnityEngine;

abstract public class AuraSpellAttribute : BaseSpellAttribute
{
    protected PlayerStatus PlayerStatus
    {
        get { return spellCaster.TryGetComponent(out PlayerStatus playerStatus) ? playerStatus : null; }
    }
    public abstract void CancelSpell(PlayerController playerController);

    public GameObject InstantiateSpell()
    {
        if (spellPrefab == null) return null;

        Vector3 position = CasterPosition;

        position.y += 0.8f;

        return Instantiate(spellPrefab, position, Quaternion.identity);
    }
}