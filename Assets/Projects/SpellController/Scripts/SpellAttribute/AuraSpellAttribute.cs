using UnityEngine;

abstract public class AuraSpellAttribute : BaseSpellAttribute
{

    public abstract void CancelSpell();

    public GameObject InstantiateSpell()
    {
        if (spellPrefab == null) return null;

        Vector3 position = CasterPosition;

        position.y += 0.8f;

        GameObject gameObject = Instantiate(spellPrefab, position, Quaternion.identity);

        BaseSpellAttribute[] baseSpellAttributes = gameObject.GetComponents<BaseSpellAttribute>();
        foreach (var baseSpellAttribute in baseSpellAttributes)
        {
            if (baseSpellAttribute != null)
            {
                baseSpellAttribute.CopyFrom(this);
            }
        }
        return gameObject;
    }
}