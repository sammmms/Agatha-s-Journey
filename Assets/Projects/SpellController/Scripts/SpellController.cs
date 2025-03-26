using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;



public class SpellController : MonoBehaviour
{
    [SerializeField] private SpellDatabase _spellDatabase;
    /// <summary>
    ///    List of spells that can be selected by the player
    ///    - This is the list of spells that the player can select from the hotbar
    ///    - This spell can be re-arranged by the player
    /// </summary>
    public List<Spell> selectableSpell = new List<Spell>(4);

    /// <summary>
    ///   List of spells that are currently active
    /// </summary>
    public List<Spell> spells = new List<Spell>(3);

    /// <summary>
    /// List of active auras
    /// </summary>
    private Dictionary<Spell, GameObject> activeAura = new();
    private Dictionary<Spell, GameObject> shootableProjectiles = new();

    private static readonly Dictionary<Spell, Spell> twoSpellCombos = new()
    {
        {Spell.Barrier | Spell.Bolt, Spell.ShockingBarrier},
        {Spell.Barrier | Spell.Enhance, Spell.ReinforcedBarrier},
        {Spell.Enhance | Spell.Heal, Spell.AmplifiedRecovery},
        {Spell.Enhance | Spell.Lightweight, Spell.HighJump},
        {Spell.Flare | Spell.Bolt, Spell.BlazingSurge},
        {Spell.Arrow | Spell.Bolt, Spell.LightningBolt},
        {Spell.Arrow | Spell.Enhance, Spell.PiercingShot},
        {Spell.Arrow | Spell.Flare, Spell.ExplosiveShot},
        {Spell.Heal | Spell.Arrow, Spell.HolyShot},
    };

    private static readonly HashSet<Spell> auraSpell = new()
    {
        Spell.Barrier,
        Spell.ReinforcedBarrier,
        Spell.ShockingBarrier,
        Spell.ShockwaveBarrier,
        Spell.Heal,
        Spell.AmplifiedRecovery,
        Spell.Lightweight,
        Spell.HighJump,
        Spell.Enhance,
    };

    private static readonly Dictionary<Spell, Spell> threeSpellCombos = new()
    {
        {Spell.Barrier | Spell.Bolt | Spell.Enhance, Spell.ShockwaveBarrier},
        {Spell.Arrow | Spell.Flare | Spell.Bolt, Spell.FirestormVolley},
        {Spell.Heal | Spell.Enhance | Spell.Arrow, Spell.PurifyingBlizzard},
        {Spell.Heal | Spell.Enhance | Spell.Flare, Spell.SurgeOfBlessing},
    };

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
        if (_spellLocomotionInput != null)
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
        Spell spell = selectableSpell[index - 1];

        ToggleHotbarItem(spell);

        List<Spell> spellToActivate = HandleCombinationSpell();

        print($"spell to remove : {spellToActivate}");

        List<Spell> spellToRemove = new List<Spell>();
        foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
        {
            if (!spellToActivate.Contains(kvp.Key))
            {
                spellToRemove.Add(kvp.Key);
                print($"spellToRemove: {kvp.Key}");
            }
        }

        foreach (Spell spellToRemoveKey in spellToRemove)
        {
            Destroy(activeAura[spellToRemoveKey]);
            activeAura.Remove(spellToRemoveKey);
        }

        foreach (Spell _spell in spellToActivate)
        {
            print($"spellToActivate: {_spell}");
            SetSpellAsActive(_spell);
        }
    }

    private List<Spell> HandleCombinationSpell()
    {
        List<Spell> newActiveSpell = new();

        if (spells.Count == 0)
        {
            return newActiveSpell;
        }

        if (spells.Count == 1)
        {
            newActiveSpell.Add(spells[0]);
            return newActiveSpell;
        }


        // Check three spell combos
        if (spells.Count == 3)
        {
            Spell threeSpellCombo = (Spell)spells[0] | (Spell)spells[1] | (Spell)spells[2];

            if (threeSpellCombos.ContainsKey(threeSpellCombo))
            {
                newActiveSpell.Add(threeSpellCombos[threeSpellCombo]);
                return newActiveSpell;
            }
        }


        List<Spell> combinedSpell = new(spells);
        foreach (Spell spell in spells)
        {
            foreach (Spell combo in spells)
            {
                Spell twoSpellCombo = spell | combo;

                if (twoSpellCombos.ContainsKey(twoSpellCombo) && !newActiveSpell.Contains(twoSpellCombos[twoSpellCombo]))
                {
                    newActiveSpell.Add(twoSpellCombos[twoSpellCombo]);
                    combinedSpell.Remove(spell);
                    combinedSpell.Remove(combo);
                    break;
                }
            }

            if (combinedSpell.Count < 3)
            {
                break;
            }
        }

        foreach (Spell spell in combinedSpell)
        {
            newActiveSpell.Add(spell);
        }

        return newActiveSpell;
    }

    private void SetSpellAsActive(Spell spell)
    {
        if (activeAura.ContainsKey(spell) || shootableProjectiles.ContainsKey(spell))
        {
            return;
        }

        GameObject spellPrefab = GetGameObject(spell);
        bool isAura = auraSpell.Contains(spell);

        if (isAura)
        {
            GameObject aura = Instantiate(spellPrefab, transform.position, Quaternion.identity);
            activeAura.Add(spell, aura);
        }
        else
        {
            shootableProjectiles.Add(spell, spellPrefab);
        }
    }


    private void ToggleHotbarItem(Spell spell)
    {
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

    private GameObject GetGameObject(Spell spell)
    {
        if (spell == Spell.None) return null;

        return _spellDatabase.GetSpellPrefab(spell);
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
        foreach (KeyValuePair<Spell, GameObject> kvp in shootableProjectiles)
        {
            HandleLaunchSpell(kvp.Value);
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
