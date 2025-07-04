using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Sound Related")]
    [SerializeField] public AudioClip damageSound;
    [SerializeField] public AudioClip deathSound;

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
    private PlayerAnimation _playerAnimation;

    private bool _isDead = false;
    public bool IsDead => _isDead;
    private void Start()
    {
        // Get the PlayerState component from the GameObject
        _playerState = GetComponent<PlayerState>();
        // Get the PlayerAnimation component from the GameObject
        _playerAnimation = GetComponent<PlayerAnimation>();

        // Initialize health, mana, and stamina to their maximum values
        SetMaxHealth(maxHealthPoint);
        SetMaxMana(maxManaPoint);
        SetMaxStamina(maxStaminaPoint);



        // Start the regeneration process
        InitiateRegen();
    }

    #region UI Methods
    private void InitiateRegen()
    {
        if (_regenCoroutine == null)
        {
            _regenCoroutine = StartCoroutine(RegenStatus());
        }
    }

    private void StopRegen()
    {
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
            _regenCoroutine = null;
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

        if (currentHealthPoint <= 0)
        {
            HandleDeath();
        }
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

        if (damage <= 0 || currentHealthPoint <= 0)
        {
            return;
        }

        // Play damage sound if assigned
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }


        SetHealth(currentHealthPoint - damage);
        _playerAnimation.PlayHitAnimation();
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

    #region Buff

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

    public void ApplyJumpBuff(float jumpBuff)
    {
        jumpSpeed += jumpBuff;
    }

    public void RemoveJumpBuff(float jumpBuff)
    {
        jumpSpeed -= jumpBuff;
    }

    public void ApplyGravityBuff(float gravityBuff)
    {
        gravity -= gravityBuff;
    }

    public void RemoveGravityBuff(float gravityBuff)
    {
        gravity += gravityBuff;
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

    #endregion

    private void HandleDeath()
    {
        _isDead = true;
        StopRegen();
        // Handle player death (e.g., play animation, respawn, etc.)
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        _playerAnimation.PlayDeathAnimation();

    }
}
