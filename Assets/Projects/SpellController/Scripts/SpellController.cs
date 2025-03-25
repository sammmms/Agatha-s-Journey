using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;

public enum Spell
{
    Lightweight = 1,
    Barrier = 2,
    Enhance = 3,
    Heal = 4,
    Bolt = 5,
    Flare = 6,
    Arrow = 7,

    // Upgraded spells
    GravityManipulation = 8, // Lightweight (Upgraded)
    ManaConstruct = 9, // Barrier (Upgraded)

    // Enhanced spells
    HighJump = 10, // Lightweight + Enhance
    ShockwaveBarrier = 11, // Barrier + Bolt
    PiercingShot = 12, // Arrow + Enhance
    ReinforcedBarrier = 13, // Barrier + Enhance
    BlazingSurge = 14, // Flare + Bolt
    ExplosiveShot = 15, // Arrow + Flare
    AmplifiedRecovery = 16, // Heal + Enhance
    HolyShot = 17, // Heal + Arrow

    // Conjurable Three-Spell Combinations
    FirestormVolley = 18, // Arrow + Flare + Bolt
    HoverStep = 19, // Lightweight (Upgraded) + Enhance
    FloatingPlatform = 20, // Lightweight (Upgraded) + Barrier (Upgraded)
    SturdyShield = 21, // Barrier (Upgraded) + Enhance
    FortifiedFloatingPlatform = 22, // Lightweight (Upgraded) + Barrier (Upgraded) + Enhance

    // Unconjurable Two-Spell Combinations & Negative Effects
    OverchargedBody = 23, // Lightweight (Upgraded) + Bolt (Stun for [certain seconds] if trying to cast [LeftClick])
    ManaBurn = 24, // Barrier (Upgraded) + Flare (Take [certain damage and mana] if trying to cast)
    PainSurge = 25, // Heal + Bolt (Stun for [certain seconds] if trying to cast [LeftClick])
    CorruptRegeneration = 26, // Heal + Flare (Take [certain mana] if trying to cast [LeftClick])
    ArrowRebound = 27, // Arrow + Barrier (Take [certain damage] if trying to cast [LeftClick])

    // Unconjurable Three-Spell Combinations & Negative Effects
    OverchargedHeart = 28, // Lightweight (Upgraded) + Heal + Bolt (Stun for [more seconds] if trying to cast [LeftClick])
    ManaOverload = 29 // Heal + Barrier (Upgraded) + Bolt (Take [more damage and mana] and stun [for brief seconds] if trying to cast)   
}

public class SpellController : MonoBehaviour
{
    public List<Spell> selectableSpell = new List<Spell>();

    public List<Spell> spells = new List<Spell>();

    public Dictionary<Spell, GameObject> activeAura = new Dictionary<Spell, GameObject>();

    [Header("Basic Spells")]
    [SerializeField] private GameObject lightweightAura;
    [SerializeField] private GameObject barrierAura;
    [SerializeField] private GameObject enhanceAura;
    [SerializeField] private GameObject healAura;
    [SerializeField] private GameObject bolt;
    [SerializeField] private GameObject flare;
    [SerializeField] private GameObject arrow;

    [Header("Upgraded Spells")]
    [SerializeField] private GameObject gravityManipulation;
    [SerializeField] private GameObject manaConstruct;

    [Header("Enhanced Spells")]
    [SerializeField] private GameObject highJump;
    [SerializeField] private GameObject shockwaveBarrier;
    [SerializeField] private GameObject piercingShot;
    [SerializeField] private GameObject reinforcedBarrier;
    [SerializeField] private GameObject blazingSurge;
    [SerializeField] private GameObject explosiveShot;
    [SerializeField] private GameObject amplifiedRecovery;
    [SerializeField] private GameObject holyShot;

    [Header("Conjurable Three-Spell Combinations")]
    [SerializeField] private GameObject firestormVolley;
    [SerializeField] private GameObject hoverStep;
    [SerializeField] private GameObject floatingPlatform;
    [SerializeField] private GameObject sturdyShield;
    [SerializeField] private GameObject fortifiedFloatingPlatform;

    private SpellLocomotionInput _spellLocomotionInput;

    private PlayerState _playerState;

    private Camera _playerCamera;

    [SerializeField] private float spellSpeed = 10f;

    private void Awake()
    {
        _spellLocomotionInput = GetComponent<SpellLocomotionInput>();
        _spellLocomotionInput.OnSpellSelected += HandleHotbarInput;
        _spellLocomotionInput.OnShootTriggered += HandlePlayerClick;

        _playerState = GetComponent<PlayerState>();
    }

    private void OnDestroy()
    {
        if(_spellLocomotionInput != null)
        {
            _spellLocomotionInput.OnSpellSelected -= HandleHotbarInput;
        }

    }

    private void Start()
    {
        _playerCamera = Camera.main;
    }

    private void Update()
    {
        HandleActiveAura();
    }

    private void HandleHotbarInput(int index)
    {
        Spell spell = selectableSpell[index-1];

        ToggleHotbarItem(spell);

        HandleCombinedAura();
    }

    private void HandleCombinedAura()
    {
        List<Spell> auraSpells = new List<Spell>(spells);
        foreach (Spell spell in spells) {
            bool isCombined = false;
            foreach (Spell anotherSpell in spells) 
            { 
                if(spell != anotherSpell)
                {
                    if (spell == Spell.Lightweight && anotherSpell == Spell.Enhance)
                    {
                        auraSpells.Add(Spell.HighJump);
                        isCombined = true;
                    }
                    if (spell == Spell.Barrier && anotherSpell == Spell.Bolt)
                    {
                        auraSpells.Add(Spell.ShockwaveBarrier);
                        isCombined = true;
                    }
                    if (spell == Spell.Barrier && anotherSpell == Spell.Enhance)
                    {
                        auraSpells.Add(Spell.ReinforcedBarrier);
                        isCombined = true;
                    }
                    if (spell == Spell.Heal && anotherSpell == Spell.Enhance)
                    {
                        auraSpells.Add(Spell.AmplifiedRecovery);
                        isCombined = true;
                    }

                    if (isCombined)
                    {
                        auraSpells.Remove(spell);
                        auraSpells.Remove(anotherSpell);
                        break;
                    }
                }
            }
        }

        List<Spell> currentActiveAura = new List<Spell>(activeAura.Keys);

        foreach (Spell spell in currentActiveAura)
        {
            if (!auraSpells.Contains(spell))
            {
                Destroy(activeAura[spell]);
                activeAura.Remove(spell);
            }
        }

        foreach (Spell spell in auraSpells)
        {
            if (!currentActiveAura.Contains(spell))
            {
                HandleAuraSpell(spell);
            }
        }


    }

    private void ToggleHotbarItem(Spell spell) {
        if (spells.Contains(spell))
        {
            spells.Remove(spell);
        }
        else
        {
            spells.Add(spell);
        }

        print($"Selected spells: {string.Join(", ", spells)}");
    }

    private void HandleActiveAura()
    {
        foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
        {
            Vector3 position = transform.position;

            bool isBarrierRelated = kvp.Key == Spell.Barrier || kvp.Key == Spell.ReinforcedBarrier || kvp.Key == Spell.ShockwaveBarrier;

            position.y += isBarrierRelated ? 0.8f : 0f;
            kvp.Value.transform.position = position;
        }
    }

    private void HandleAuraSpell(Spell spell)
    {
        GameObject auraSpell = GetAuraObject(spell);

        if (auraSpell != null)
        {
           activeAura[spell] = Instantiate(auraSpell, transform.position, Quaternion.identity);
        }
    }

    private GameObject GetAuraObject(Spell spell)
    {
        switch (spell)
        {
            case Spell.Lightweight:
                return lightweightAura;
            case Spell.Barrier:
                return barrierAura;
            case Spell.Enhance:
                return enhanceAura;
            case Spell.Heal:
                return healAura;
            default:
                return GetEnhancedAuraObject(spell);
        }
    }

    private GameObject GetEnhancedAuraObject(Spell spell)
    {
        switch (spell)
        {
            case Spell.HighJump:
                return highJump;
            case Spell.ShockwaveBarrier:
                return shockwaveBarrier;
            case Spell.ReinforcedBarrier:
                return reinforcedBarrier;
            case Spell.AmplifiedRecovery:
                return amplifiedRecovery;
            default:
                return null;
        }
    }

    private void HandlePlayerClick(ShootMode shootMode)
    {
        if (shootMode == ShootMode.AutoTarget)
        {
            HandlePlayerShoot();
        }
        else if (shootMode == ShootMode.ManualAim)
        {
            HandlePlayerAiming();
        }
    }

    private void HandlePlayerAiming()
    {
        PlayerMovementState currentState = _playerState.CurrentPlayerMovementState;

        if (currentState == PlayerMovementState.Aiming)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Idling);
        }
        else
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Aiming);
        }
    }

    private void HandlePlayerShoot()
    {
        if(spells.Contains(Spell.Bolt))
        {
            GameObject boltSpell = Instantiate(bolt, transform.position, Quaternion.identity);
            HandleLaunchSpell(boltSpell);
        }
        else if (spells.Contains(Spell.Flare))
        {
            GameObject flareSpell = Instantiate(flare, transform.position, Quaternion.identity);
            HandleLaunchSpell(flareSpell);
        }
        else if (spells.Contains(Spell.Arrow))
        {
            GameObject arrowSpell = Instantiate(arrow, transform.position, Quaternion.identity);
            HandleLaunchSpell(arrowSpell);
        }
    }

    private void HandleLaunchSpell(GameObject spell)
    {
        // Get direction using camera
        Vector3 targetPosition = _playerCamera.transform.position + _playerCamera.transform.forward * 10f;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Rigidbody spellRb = spell.GetComponent<Rigidbody>();

        spellRb.linearVelocity = direction * spellSpeed;

        print($"Launched : {spell.name} to {spellRb.linearVelocity}");
    }
}
