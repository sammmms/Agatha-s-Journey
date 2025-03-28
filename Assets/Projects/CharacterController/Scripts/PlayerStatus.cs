using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public HealthBar healthBar;
    [SerializeField] public ManaBar manaBarPrefab;
    [SerializeField] public StaminaBar staminaBarPrefab;

    [Header("Player Control Status")]
    [SerializeField] public float runSpeed = 4f;
    [SerializeField] public float sprintSpeed = 8f;
    [SerializeField] public float jumpSpeed = 1.0f;
    [SerializeField] public float gravity = 25f;

    [Header("Player Status")]
    [SerializeField] public float maxHealthPoint = 100;
    [SerializeField] public float currentHealthPoint = 100;

    [SerializeField] public float maxManaPoint = 100;
    [SerializeField] public float currentManaPoint = 100;

    [SerializeField] public float maxStaminaPoint = 100;
    [SerializeField] public float currentStaminaPoint = 100;

    [Header("Regen Rate")]
    [SerializeField] public float healthRegenRate = 1;
    [SerializeField] public float manaRegenRate = 1;
    [SerializeField] public float staminaRegenRate = 1;
    [SerializeField] private float regenRate = 1f;

    [Header("Damage Reduction")]
    [SerializeField] float damageReduction = 0;

    [Header("Damage Buff")]
    [SerializeField] float damageBuff = 0;

    [Header("Heal Buff")]
    [SerializeField] float healBuff = 0;
    [SerializeField] float healRate = 1f; // Heal every second

    // Regen every second
    private Coroutine _regenCoroutine;
    private Coroutine _healCoroutine;
    private PlayerState _playerState;
    private void Start()
    {
        // Get the PlayerState component from the GameObject
        _playerState = GetComponent<PlayerState>();

        // Initialize health, mana, and stamina to their maximum values
        SetMaxHealth(maxHealthPoint);
        SetMaxMana(maxManaPoint);
        SetMaxStamina(maxStaminaPoint);



        // Start the regeneration process
        initiateRegen();
    }

    #region UI Methods
    private void initiateRegen()
    {
        if (_regenCoroutine == null)
        {
            _regenCoroutine = StartCoroutine(RegenStatus());
        }
    }

    private IEnumerator RegenStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenRate);

            RegenHealth();
            RegenMana();
            RegenStamina();
        }
    }

    private void RegenHealth()
    {
        if (currentHealthPoint < maxHealthPoint)
        {
            SetHealth(currentHealthPoint + healthRegenRate);
        }
    }

    private void RegenMana()
    {
        if (currentManaPoint < maxManaPoint)
        {
            SetMana(currentManaPoint + manaRegenRate);
        }
    }

    private void RegenStamina()
    {
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        if (currentStaminaPoint < maxStaminaPoint && !isSprinting)
        {
            SetStamina(currentStaminaPoint + staminaRegenRate);
        }
    }
    #endregion

    #region Setter
    public void SetMaxHealth(float maxHealth)
    {
        maxHealthPoint = maxHealth;
        healthBar.SetMaxHealth(maxHealthPoint);

        SetHealth(maxHealthPoint);
    }
    public void SetMaxMana(float maxMana)
    {
        maxManaPoint = maxMana;
        manaBarPrefab.SetMaxMana(maxManaPoint);

        SetMana(maxManaPoint);
    }

    public void SetMaxStamina(float maxStamina)
    {
        maxStaminaPoint = maxStamina;
        staminaBarPrefab.SetMaxStamina(maxStaminaPoint);

        SetStamina(maxStaminaPoint);
    }

    public void SetHealth(float health)
    {
        currentHealthPoint = health;
        healthBar.SetHealth(currentHealthPoint);
    }

    public void SetMana(float mana)
    {
        currentManaPoint = mana;
        manaBarPrefab.SetMana(currentManaPoint);
    }

    public void SetStamina(float stamina)
    {
        currentStaminaPoint = stamina;
        staminaBarPrefab.SetStamina(currentStaminaPoint);
    }
    #endregion

    public void TakeDamage(float damage)
    {
        damage -= damageReduction;

        if (damage < 0)
        {
            damage = 0;
        }

        SetHealth(currentHealthPoint - damage);
    }

    public void TakeMana(float mana)
    {
        if (currentManaPoint <= 0)
        {
            return;
        }

        if (currentManaPoint - mana < 0)
        {
            mana = currentManaPoint;
        }

        SetMana(currentManaPoint - mana);
    }

    public void TakeStamina(float stamina)
    {
        if (currentStaminaPoint <= 0)
        {
            return;
        }

        if (currentStaminaPoint - stamina < 0)
        {
            stamina = currentStaminaPoint;
        }

        SetStamina(currentStaminaPoint - stamina);
    }

    public void Heal(float heal)
    {
        if (currentHealthPoint >= maxHealthPoint)
        {
            return;
        }

        if (currentHealthPoint + heal > maxHealthPoint)
        {
            heal = maxHealthPoint - currentHealthPoint;
        }

        SetHealth(currentHealthPoint + heal);
    }

    public void ApplyDamageBuff(float buff)
    {
        damageBuff += buff;
    }

    public void RemoveDamageBuff(float buff)
    {
        damageBuff -= buff;
    }

    public void ApplyDamageReduction(float reduction)
    {
        damageReduction += reduction;
    }

    public void RemoveDamageReduction(float reduction)
    {
        damageReduction -= reduction;
    }

    public void ApplySpeedBuff(float speedBuff)
    {
        runSpeed += speedBuff;
        sprintSpeed += speedBuff;
    }

    public void RemoveSpeedBuff(float speedBuff)
    {
        runSpeed -= speedBuff;
        sprintSpeed -= speedBuff;
    }

    public void ApplyJumpBuff(float jumpBuff, float gravityBuff)
    {
        jumpSpeed += jumpBuff;
        gravity += gravityBuff;
    }

    public void RemoveJumpBuff(float jumpBuff, float gravityBuff)
    {
        jumpSpeed -= jumpBuff;
        gravity -= gravityBuff;
    }

    public void ApplyHealBuff(float healBuff)
    {
        this.healBuff += healBuff;
        if (_healCoroutine == null)
        {
            _healCoroutine = StartCoroutine(HealOverTime());
        }
    }

    public void RemoveHealBuff(float healBuff)
    {
        this.healBuff -= healBuff;
        if (_healCoroutine != null)
        {
            StopCoroutine(_healCoroutine);
            _healCoroutine = null;
        }
    }
    private IEnumerator HealOverTime()
    {
        while (true)
        {
            Heal(healBuff);
            yield return new WaitForSeconds(1f);
        }
    }
}
