using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Spell
{
    None = 0,
    // Basic Spells
    Lightweight = 1,
    Barrier = 2,
    Enhance = 4,
    Heal = 8,
    Bolt = 16,
    Flare = 32,
    Arrow = 64,

    // Upgraded Spells
    GravityManipulation = Lightweight * 128,
    ManaConstruct = Barrier * 128,

    // Conjurable Two-Spell Combinations
    ShockingBarrier = Barrier + Bolt,
    ReinforcedBarrier = Barrier + Enhance,
    AmplifiedRecovery = Enhance + Heal,
    HighJump = Enhance + Lightweight,
    OverloadBall = Flare + Bolt,
    LightningBolt = Arrow + Bolt,
    PiercingShot = Arrow + Enhance,
    ExplosiveShot = Arrow + Flare,
    HolyShot = Heal + Arrow,

    // Conjurable Three-Spell Combinations
    ShockwaveBarrier = Barrier + Bolt + Enhance,
    FirestormVolley = Arrow + Flare + Bolt,
    PurifyingBlizzard = Heal + Enhance + Arrow,
    SurgeOfBlessing = Heal + Enhance + Flare,

    // Other Combinations
    PainSurge = Heal + Bolt,
    CorruptRegeneration = Heal + Flare,
    ArrowRebound = Arrow + Barrier,
}

[CreateAssetMenu(fileName = "SpellDatabase", menuName = "Spells/Spell Database")]
public class SpellDatabase : ScriptableObject
{
    [Header("Spell Data")]
    private List<Spell> _basicSpells = new List<Spell>
    {
        Spell.Lightweight,
        Spell.Barrier,
        Spell.Enhance,
        Spell.Heal,
        Spell.Bolt,
        Spell.Flare,
        Spell.Arrow
    };

    [System.Serializable]
    public struct SpellData
    {
        public Spell spell;
        public GameObject spellPrefab;
    }

    public List<SpellData> spellData = new List<SpellData>();

    public GameObject GetSpellPrefab(Spell spell)
    {
        foreach (var data in spellData)
        {
            if (data.spell == spell)
            {
                return data.spellPrefab;
            }
        }

        return null;
    }

    public List<GameObject> GetAllBasicSpellsPrefab()
    {
        List<GameObject> basicSpellPrefabs = new List<GameObject>();
        foreach (var spell in _basicSpells)
        {
            GameObject prefab = GetSpellPrefab(spell);
            if (prefab != null)
            {
                basicSpellPrefabs.Add(prefab);
            }
            else
            {
                Debug.LogWarning($"No prefab found for spell: {spell}");
            }
        }
        return basicSpellPrefabs;
    }

    public List<Spell> GetBasicSpellsOf(Spell spell)
    {
        List<Spell> basicSpells = new List<Spell>();
        foreach (var basicSpell in _basicSpells)
        {
            if ((spell & basicSpell) == basicSpell)
            {
                basicSpells.Add(basicSpell);
            }
        }
        return basicSpells;
    }
}
