using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;

public enum Spell{
    Lightweight = 1,
    Barrier = 2,
    Enhance = 3,
    Heal = 4,
    Bolt = 5,
    Flare = 6,
    Arrow = 7,

    // Enhanced spells
}

public class SpellController : MonoBehaviour
{
    public List<Spell> selectableSpell = new List<Spell>();

    public List<Spell> spells = new List<Spell>();

    public Dictionary<Spell, GameObject> activeAura = new Dictionary<Spell, GameObject>();

    [SerializeField] private GameObject lightweightAura;
    [SerializeField] private GameObject barrierAura;
    [SerializeField] private GameObject enhanceAura;
    [SerializeField] private GameObject healAura;
    [SerializeField] private GameObject bolt;
    [SerializeField] private GameObject flare;
    [SerializeField] private GameObject arrow;

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

        selectableSpell.Add(Spell.Lightweight);
        selectableSpell.Add(Spell.Barrier);
        selectableSpell.Add(Spell.Enhance);
        selectableSpell.Add(Spell.Heal);
    }

    private void Update()
    {
        HandleActiveAura();
    }

    private void HandleHotbarInput(int index)
    {
        Spell spell = selectableSpell[index-1];

        ToggleHotbarItem(spell);

        HandleAuraSpell(spell);
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
            kvp.Value.transform.position = transform.position;
        }
    }

    private void HandleAuraSpell(Spell spell)
    {
        if (activeAura.ContainsKey(spell))
        {
            Destroy(activeAura[spell]);
            activeAura.Remove(spell);
            return;
        }

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
