using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpellController : MonoBehaviour
{
    [SerializeField] private SpellDatabase _spellDatabase;
    private SpellLocomotionInput _spellLocomotionInput;
    private PlayerState _playerState;
    private PlayerStatus _playerStatus;
    public List<Spell> hotbarSpell = new List<Spell>()
    {
        Spell.Lightweight,
        Spell.Bolt,
    };

    public List<Spell> activeSpells = new List<Spell>();
    private Dictionary<Spell, GameObject> activeAura = new();
    private Dictionary<Spell, GameObject> shootableProjectiles = new();
    private Dictionary<Spell, float> spellCooldown = new();
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

    private void Awake()
    {
        _spellLocomotionInput = GetComponent<SpellLocomotionInput>();
        _spellLocomotionInput.OnSpellSelected += HandleHotbarInput;
        _spellLocomotionInput.OnShootTriggered += HandlePlayerClick;

        _playerState = GetComponent<PlayerState>();
        _playerStatus = GetComponent<PlayerStatus>();


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

    private void HandleHotbarInput(int index)
    {
        Spell spell = hotbarSpell[index - 1];

        ToggleHotbarItem(spell);

        HandleSpellCombination();
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

        // TODO: Handle UI //
    }

    #endregion

    private void HandleSpellCombination()
    {
        List<Spell> spellToActivate = GetSpellCombination();


        HashSet<Spell> activeSpells = new HashSet<Spell>(activeAura.Keys);
        activeSpells.UnionWith(shootableProjectiles.Keys);

        HashSet<Spell> spellToRemove = new HashSet<Spell>();
        foreach (Spell spell in activeSpells)
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
    private void HandleActiveAura()
    {
        HashSet<Spell> spellToRemove = new();
        foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
        {
            Vector3 position = transform.position;

            BaseSpellAttribute spellAttribute = kvp.Value.GetComponent<BaseSpellAttribute>();

            bool isBarrierRelated = spellAttribute is BarrierSpellAttribute;

            position.y += isBarrierRelated ? 0.8f : 0f;
            kvp.Value.transform.position = position;
        }

        if (spellToRemove.Count == 0)
        {
            return;
        }

        List<Spell> spellToActivate = new();
        foreach (Spell spell in spellToRemove)
        {
            HandleAuraDeactivation(GetGameObject(spell));

            List<Spell> baseSpell = GetBaseSpellOf(spell);

            foreach (Spell baseSpellItem in baseSpell)
            {
                spellToActivate.Add(baseSpellItem);
            }
        }

        foreach (Spell spell in spellToActivate)
        {
            HandleSpellActivation(spell);
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
            HandleAuraDeactivation(GetGameObject(spell));
        }
        else if (shootableProjectiles.ContainsKey(spell))
        {
            shootableProjectiles.Remove(spell);
        }
    }

    private List<Spell> GetBaseSpellOf(Spell spell)
    {
        List<Spell> baseSpell = new();

        foreach (KeyValuePair<Spell, Spell> kvp in twoSpellCombos)
        {
            if (kvp.Value == spell)
            {
                baseSpell.Add(kvp.Key);
                break;
            }
        }

        foreach (KeyValuePair<Spell, Spell> kvp in threeSpellCombos)
        {
            if (kvp.Value == spell)
            {
                baseSpell.Add(kvp.Key);
                break;
            }
        }

        return baseSpell;
    }

    private GameObject GetGameObject(Spell spell)
    {
        if (spell == Spell.None) return null;

        return _spellDatabase.GetSpellPrefab(spell);
    }

    #region Activation Method
    private void HandleAuraActivation(GameObject spellPrefab)
    {
        BaseSpellAttribute spellAttribute = spellPrefab.GetComponent<BaseSpellAttribute>();

        if (spellAttribute == null)
        {
            return;
        }

        GameObject instance = HandleCastSpell(spellAttribute);

        activeAura.Add(spellAttribute.spell, instance);
        StartManaDrain();
    }

    private void HandleAuraDeactivation(GameObject spellPrefab)
    {
        BaseSpellAttribute spellAttribute = spellPrefab.GetComponent<BaseSpellAttribute>();


        bool isAura = spellAttribute is AuraSpellAttribute;
        if (!isAura)
        {
            return;
        }

        Spell spell = spellAttribute.spell;
        Destroy(activeAura[spell]);
        activeAura.Remove(spell);

        (spellAttribute as AuraSpellAttribute).cancelSpell(GetComponent<PlayerController>());
    }

    private void StartManaDrain()
    {
        print(_isDrainingMana);
        if (_isDrainingMana) return;

        _isDrainingMana = true;
        _consumeManaCoroutine = StartCoroutine(ConsumeAuraMana());
    }
    private IEnumerator ConsumeAuraMana()
    {
        while (activeAura.Count > 0)
        {
            foreach (KeyValuePair<Spell, GameObject> kvp in activeAura)
            {
                BaseSpellAttribute spellAttribute = kvp.Value.GetComponent<BaseSpellAttribute>();

                if (spellAttribute == null)
                {
                    continue;
                }

                if (_playerStatus.currentManaPoint < spellAttribute.spellCost)
                {
                    HandleSpellDeactivation(kvp.Key);
                    yield break;
                }

                _playerStatus.TakeMana(spellAttribute.spellCost);
            }

            yield return new WaitForSeconds(1f);
        }

        print("Stop Coroutine");
        _isDrainingMana = false;
        StopCoroutine(_consumeManaCoroutine);
    }


    private void HandlePlayerShoot()
    {
        foreach (KeyValuePair<Spell, GameObject> kvp in shootableProjectiles)
        {
            HandleLaunchSpell(kvp.Value);
        }
    }

    private void HandleLaunchSpell(GameObject spellPrefab)
    {
        BaseSpellAttribute spellAttribute = spellPrefab.GetComponent<BaseSpellAttribute>();

        if (spellAttribute == null)
        {
            return;
        }

        Spell spell = spellAttribute.spell;

        if (spellCooldown.ContainsKey(spell))
        {
            bool canCast = spellAttribute.canCastSpell(spellCooldown[spell], _playerStatus.currentManaPoint);

            if (!canCast)
            {
                return;
            }
        }

        _playerStatus.TakeMana(spellAttribute.spellCost);

        HandleCastSpell(spellAttribute);
        spellCooldown[spell] = spellAttribute.spellCooldown;
    }

    private GameObject HandleCastSpell(BaseSpellAttribute spellAttribute)
    {
        PlayerController playerController = GetComponent<PlayerController>();

        return spellAttribute.castSpell(playerController);
    }
    #endregion
}
