using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class SpellController : MonoBehaviour
{
    [SerializeField] private SpellDatabase _spellDatabase;
    private SpellLocomotionInput _spellLocomotionInput;
    private PlayerStatus _playerStatus;
    private PlayerState _playerState;
    private PlayerAnimation _playerAnimation;
    public List<Spell> hotbarSpell = new List<Spell>()
    {
        Spell.Lightweight,
        Spell.Bolt,
    };

    public List<Spell> activeSpells = new List<Spell>();
    private Dictionary<Spell, GameObject> activeAura = new();

    private Dictionary<Spell, GameObject> shootableProjectiles = new();
    public Dictionary<Spell, float> spellCooldown = new();
    private static readonly Dictionary<Spell, Spell> twoSpellCombos = new()
    {
        {Spell.Barrier | Spell.Bolt, Spell.ShockingBarrier},
        {Spell.Barrier | Spell.Enhance, Spell.ReinforcedBarrier},
        {Spell.Enhance | Spell.Heal, Spell.AmplifiedRecovery},
        {Spell.Enhance | Spell.Lightweight, Spell.HighJump},
        {Spell.Flare | Spell.Bolt, Spell.OverloadBall},
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

    private Coroutine _consumeManaCoroutine;
    private bool _isDrainingMana = false;

    // Add this near your other private variables
    private bool _isCooldownCoroutineRunning = false;

    private void Start()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void OnDisable()
    {
        if (_spellLocomotionInput != null)
        {
            _spellLocomotionInput.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_spellLocomotionInput != null)
        {
            _spellLocomotionInput.enabled = true;
        }


    }

    private void Awake()
    {
        _spellLocomotionInput = GetComponent<SpellLocomotionInput>();
        _spellLocomotionInput.OnSpellSelected += HandleHotbarInput;
        _spellLocomotionInput.OnShootTriggered += HandlePlayerClick;

        _playerStatus = GetComponent<PlayerStatus>();
        _playerState = GetComponent<PlayerState>();

    }

    private void OnDestroy()
    {
        if (_spellLocomotionInput != null)
        {
            _spellLocomotionInput.OnSpellSelected -= HandleHotbarInput;
        }

    }

    private void Update()
    {
        HandleActiveAura();
    }

    #region Player Inputs
    private void HandlePlayerClick()
    {
        HandlePlayerShoot();
    }

    public void AddSpellToHotbar(Spell spell)
    {
        if (hotbarSpell.Contains(spell))
        {
            Debug.LogWarning($"Spell {spell} is already in the hotbar.");
            return;
        }

        if (hotbarSpell.Count(spellItem => spellItem != Spell.None) >= 4)
        {
            Debug.LogWarning("Hotbar is full. Cannot add more spells.");
            return;
        }

        var spellIndex = hotbarSpell.IndexOf(Spell.None);
        if (spellIndex != -1)
        {
            hotbarSpell[spellIndex] = spell;
            return;
        }

        hotbarSpell.Add(spell);
        Debug.Log($"Added {spell} to hotbar.");
    }

    public void RemoveSpellFromHotbar(Spell spell)
    {
        if (!hotbarSpell.Contains(spell))
        {
            Debug.LogWarning($"Spell {spell} is not in the hotbar.");
            return;
        }

        hotbarSpell.Remove(spell);
        activeSpells.Remove(spell);
        HandleSpellCombination(); // Ensure the UI and active spells are updated
    }

    private void HandleHotbarInput(int index)
    {
        if (index <= 0 || index > hotbarSpell.Count)
        {
            Debug.LogWarning($"Invalid spell index {index} in HandleHotbarInput.");
            return;
        }
        Spell spell = hotbarSpell[index - 1];

        if (spell == Spell.None)
        {
            Debug.LogWarning("No spell assigned to this hotbar slot.");
            return;
        }

        ToggleHotbarItem(spell);
    }

    private void ToggleHotbarItem(Spell spell)
    {
        if (activeSpells.Contains(spell))
        {
            activeSpells.Remove(spell);
        }
        else
        {
            activeSpells.Add(spell);
        }

        HandleSpellCombination();
    }

    #endregion

    private void HandleSpellCombination()
    {
        List<Spell> spellToActivate = GetSpellCombination();


        HashSet<Spell> currentlyActiveSpells = new(activeAura.Keys);
        currentlyActiveSpells.UnionWith(shootableProjectiles.Keys);

        HashSet<Spell> spellToRemove = new();
        foreach (Spell spell in currentlyActiveSpells)
        {
            if (!spellToActivate.Contains(spell))
            {
                spellToRemove.Add(spell);
            }
        }

        foreach (Spell spell in spellToRemove)
        {
            HandleSpellDeactivation(spell);
        }

        foreach (Spell spell in spellToActivate)
        {
            HandleSpellActivation(spell);
        }
    }
    private List<Spell> GetSpellCombination()
    {
        List<Spell> newActiveSpell = new();

        if (activeSpells.Count == 0)
        {
            return newActiveSpell;
        }

        if (activeSpells.Count == 1)
        {
            newActiveSpell.Add(activeSpells[0]);
            return newActiveSpell;
        }


        // Check three spell combos
        if (activeSpells.Count == 3)
        {
            Spell threeSpellCombo = activeSpells[0] | activeSpells[1] | activeSpells[2];

            if (threeSpellCombos.ContainsKey(threeSpellCombo))
            {
                newActiveSpell.Add(threeSpellCombos[threeSpellCombo]);
                return newActiveSpell;
            }
        }


        List<Spell> combinedSpell = new(activeSpells);
        foreach (Spell spell in activeSpells)
        {
            foreach (Spell combo in activeSpells)
            {
                if (spell == combo) continue;

                Spell twoSpellCombo = spell | combo;

                if (twoSpellCombos.ContainsKey(twoSpellCombo) && !newActiveSpell.Contains(twoSpellCombos[twoSpellCombo]))
                {
                    newActiveSpell.Add(twoSpellCombos[twoSpellCombo]);
                    combinedSpell.Remove(spell);
                    combinedSpell.Remove(combo);
                    break;
                }
            }

            if (combinedSpell.Count < 2) // Adjusted logic for clarity
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
    private void HandleActiveAura()
    {
        foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
        {
            Vector3 position = transform.position;

            BaseSpellAttribute spellAttribute = kvp.Value.GetComponent<BaseSpellAttribute>();

            bool isBarrierRelated = spellAttribute is BarrierSpellAttribute;

            position.y += isBarrierRelated ? 0.8f : 0f;
            kvp.Value.transform.position = position;
        }

    }

    private void HandleSpellActivation(Spell spell)
    {
        if (activeAura.ContainsKey(spell) || shootableProjectiles.ContainsKey(spell))
        {
            return;
        }

        GameObject spellPrefab = GetGameObject(spell);
        bool isAura = auraSpell.Contains(spell);

        if (isAura)
        {
            HandleAuraActivation(spellPrefab);
        }
        else
        {
            shootableProjectiles.Add(spell, spellPrefab);
        }
    }

    private void HandleSpellDeactivation(Spell spell)
    {
        if (activeAura.ContainsKey(spell))
        {
            // The spell enum is the key.
            HandleAuraDeactivation(activeAura[spell]);
            // Remove it from the dictionary here, after it's been handled.
            activeAura.Remove(spell);
        }
        else if (shootableProjectiles.ContainsKey(spell))
        {
            shootableProjectiles.Remove(spell);
        }
    }

    private GameObject GetGameObject(Spell spell)
    {
        if (spell == Spell.None) return null;

        return _spellDatabase.GetSpellPrefab(spell);
    }

    #region Activation Method
    private void HandleAuraActivation(GameObject spellPrefab)
    {
        BaseSpellAttribute[] spellAttribute = spellPrefab.GetComponents<BaseSpellAttribute>();

        if (spellAttribute == null || spellAttribute.Length == 0)
        {
            return;
        }

        GameObject instance = null;

        foreach (AuraSpellAttribute spell in spellAttribute)
        {
            if (spell == null)
            {
                continue;
            }

            GameObject prefab = HandleCastSpell(spell);

            if (prefab != null)
            {
                instance = prefab;
            }
        }

        if (instance == null)
        {
            return;
        }

        activeAura.Add(spellAttribute[0].spell, instance);
        StartManaDrain();
    }

    private void HandleAuraDeactivation(GameObject spellInstance)
    {
        if (spellInstance == null) return;

        // Get all AuraSpellAttributes from the instance.
        AuraSpellAttribute[] auraAttributes = spellInstance.GetComponents<AuraSpellAttribute>();

        // Call CancelSpell on all of them to make sure buffs are removed.
        foreach (AuraSpellAttribute attr in auraAttributes)
        {
            attr.CancelSpell();
        }

        // Destroy the instantiated GameObject.
        Destroy(spellInstance);
    }

    private void StartManaDrain()
    {
        if (_isDrainingMana) return;

        _isDrainingMana = true;
        _consumeManaCoroutine = StartCoroutine(ConsumeAuraMana());
    }
    private IEnumerator ConsumeAuraMana()
    {
        while (activeAura.Count > 0)
        {
            bool canContinue = true;
            float totalManaCost = 0f;
            foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
            {
                if (!kvp.Value.TryGetComponent<BaseSpellAttribute>(out var spellAttribute))
                {
                    continue;
                }
                totalManaCost += spellAttribute.spellCost;
            }

            if (_playerStatus.currentManaPoint < totalManaCost)
            {
                canContinue = false;
            }
            else
            {
                _playerStatus.TakeMana(totalManaCost);
            }


            if (!canContinue)
            {
                break;
            }


            yield return new WaitForSeconds(1f);
        }

        if (activeAura.Count > 0)
        {
            DeactivateAllAura();
        }

        _isDrainingMana = false;
        _consumeManaCoroutine = null;
    }

    private void DeactivateAllAura()
    {
        // Get a list of all currently active auras to avoid modifying the dictionary while iterating.
        List<Spell> aurasToDeactivate = new List<Spell>(activeAura.Keys);

        foreach (Spell spell in aurasToDeactivate)
        {
            // This will call the new, cleaner HandleAuraDeactivation.
            HandleSpellDeactivation(spell);
        }

        // Now that all auras are gone, clear the base spells that the player selected.
        // This forces a "reset" state, which is appropriate for running out of mana.
        activeSpells.Clear();

        // We need to trigger a combination check to update the UI and projectile lists (which should now be empty).
        HandleSpellCombination();
    }


    private void HandlePlayerShoot()
    {
        List<Spell> spellToActivate = GetLaunchableSpells();

        if (spellToActivate.Count > 0 && _playerState.CanCastSpell())
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Casting);
            _playerAnimation.CastSpellAnimation();
        }
    }

    public void LaunchAnimationTriggers()
    {
        // Create a copy of the dictionary to avoid modification issues during iteration
        var projectilesToLaunch = new Dictionary<Spell, GameObject>(shootableProjectiles);

        foreach (KeyValuePair<Spell, GameObject> kvp in projectilesToLaunch)
        {
            HandleLaunchSpell(kvp.Value);
        }
    }

    private bool IsSpellLaunchable(GameObject spellPrefab)
    {
        if (spellPrefab == null) return false;

        if (!spellPrefab.TryGetComponent<BaseSpellAttribute>(out var spellAttribute))
        {
            return false;
        }

        Spell spell = spellAttribute.spell;

        // Spell is launchable if it's not on cooldown and the player has enough mana
        if (!spellCooldown.ContainsKey(spell))
        {
            return spellAttribute.CanCastSpell(_playerStatus.currentManaPoint);
        }

        return false; // It's on cooldown
    }

    private List<Spell> GetLaunchableSpells()
    {
        List<Spell> launchableSpells = new();

        foreach (KeyValuePair<Spell, GameObject> kvp in shootableProjectiles)
        {

            if (IsSpellLaunchable(kvp.Value))
            {
                launchableSpells.Add(kvp.Key);
            }

        }

        return launchableSpells;
    }

    private void HandleLaunchSpell(GameObject spellPrefab)
    {

        if (spellPrefab == null) return;
        if (spellPrefab.TryGetComponent<BaseSpellAttribute>(out var spellAttribute))
        {
            // Double-check if the spell is launchable right before casting
            if (!IsSpellLaunchable(spellPrefab)) return;

            HandleCastSpell(spellAttribute);
            _playerStatus.TakeMana(spellAttribute.spellCost);
            spellCooldown[spellAttribute.spell] = spellAttribute.spellCooldown;

            // Start the cooldown handler if it's not already active
            if (!_isCooldownCoroutineRunning)
            {
                StartCoroutine(HandleSpellCooldown());
            }

        }
        _playerState.SetPlayerMovementState(PlayerMovementState.Idling);

    }

    private GameObject HandleCastSpell(BaseSpellAttribute spellAttribute)
    {
        return spellAttribute.CastSpell(gameObject);
    }

    private IEnumerator HandleSpellCooldown()
    {
        _isCooldownCoroutineRunning = true; // Set the flag to true

        while (spellCooldown.Count > 0)
        {
            List<Spell> spellsToRemove = new List<Spell>();

            // Use a copy of the keys to allow modification during iteration
            foreach (Spell spellKey in new List<Spell>(spellCooldown.Keys))
            {
                spellCooldown[spellKey] -= Time.deltaTime;

                if (spellCooldown[spellKey] <= 0f)
                {
                    spellsToRemove.Add(spellKey);
                }
            }

            foreach (Spell spell in spellsToRemove)
            {
                spellCooldown.Remove(spell);
            }

            yield return null;
        }

        _isCooldownCoroutineRunning = false; // Reset the flag when done
    }
    #endregion
}
