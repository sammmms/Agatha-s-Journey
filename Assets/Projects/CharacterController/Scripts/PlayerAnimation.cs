using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float locomotionAnimationBlendSpeed = 4f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private static int inputXHash = Animator.StringToHash("InputX");
    private static int inputYHash = Animator.StringToHash("InputY");
    private static int inputMagnitudeHash = Animator.StringToHash("InputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isFallingHash = Animator.StringToHash("isFalling");

    private Vector3 _currentBlendInput = Vector3.zero;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isGrounded = _playerState.InGroundedState();
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;

        Vector2 inputTarget = _playerLocomotionInput.MovementInput;
        // Determine speed multiplier based on player state
        float speedMultiplier = _playerState.CurrentPlayerMovementState switch
        {
            PlayerMovementState.Walking => 0.8f,
            PlayerMovementState.ChargingMana => 0.8f,
            PlayerMovementState.Running => 1f,
            PlayerMovementState.Sprinting => 2.5f,
            _ => 1f,
        };
        inputTarget *= speedMultiplier;

        _currentBlendInput = Vector3.Lerp(
            _currentBlendInput,
            inputTarget,
            locomotionAnimationBlendSpeed * Time.deltaTime
        );

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
    }

    public void CastSpellAnimation()
    {
        _animator.SetTrigger("isCasting");
    }

    public void PlayHitAnimation()
    {
        _animator.SetTrigger("isHit");
    }

    public void PlayDeathAnimation()
    {
        _animator.SetTrigger("isDead");
    }


    public void PlayManaChargeAnimation()
    {
        _animator.SetBool("isChargingMana", true);
    }

    public void StopManaChargeAnimation()
    {
        _animator.SetBool("isChargingMana", false);
    }
}
