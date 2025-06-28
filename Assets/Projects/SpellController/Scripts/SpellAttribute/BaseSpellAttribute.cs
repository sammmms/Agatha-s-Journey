using UnityEngine;

public abstract class BaseSpellAttribute : MonoBehaviour
{
    public Spell spell;
    public GameObject spellPrefab;
    public float spellCost;
    public float spellCooldown;

    public bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public abstract GameObject CastSpell(PlayerController playerController);
}