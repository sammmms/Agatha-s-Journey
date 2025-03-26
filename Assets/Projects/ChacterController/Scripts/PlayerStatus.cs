using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{

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

/// <summary>
///   This is the damage reduction of the player, usually obtain through barrier
/// </summary>
    [SerializeField] float damageReduction = 0;
    [SerializeField] float physicalDamageReduction = 0;
    [SerializeField] float magicalDamageReduction = 0;

    // Regen every second
    private float regenRate = 1f;

    
}
