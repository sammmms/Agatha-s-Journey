using UnityEngine;

public abstract class BaseSpellAttribute : MonoBehaviour
{
    public Spell spell;
    public GameObject spellPrefab;
    public float spellCost;
    public float spellCooldown;

    public abstract bool canCastSpell(float currentCooldown, float currentMana);

    public abstract GameObject castSpell();
}