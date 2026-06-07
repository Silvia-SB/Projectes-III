using UnityEngine;

[RequireComponent(typeof(PlayerShooter))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private Animator bowAnimator;

    private PlayerShooter shooter;
    private PlayerMovement movement;

    private static readonly int isChargingHash = Animator.StringToHash("isCharging");
    private static readonly int cancelChargeHash = Animator.StringToHash("cancelCharge");
    private static readonly int shootHash = Animator.StringToHash("Shoot");
    private static readonly int changeArrowHash = Animator.StringToHash("changeArrow");
    
    private static readonly int velocityHash = Animator.StringToHash("Velocity"); 

    private void Awake()
    {
        shooter = GetComponent<PlayerShooter>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        shooter.OnChargeStart += HandleChargeStart;
        shooter.OnChargeEnd += HandleChargeEnd;
        shooter.OnChargeCanceled += HandleChargeCanceled;
        shooter.OnShootTriggered += HandleShoot;
        shooter.OnArrowChanged += HandleArrowChanged;
    }

    private void OnDisable()
    {
        shooter.OnChargeStart -= HandleChargeStart;
        shooter.OnChargeEnd -= HandleChargeEnd;
        shooter.OnChargeCanceled -= HandleChargeCanceled;
        shooter.OnShootTriggered -= HandleShoot;
        shooter.OnArrowChanged -= HandleArrowChanged;
    }

    private void Update()
    {
        if (movement != null && movement.Controller != null)
        {
            Vector3 horizontalVelocity = new Vector3(movement.Controller.velocity.x, 0f, movement.Controller.velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            if (armsAnimator != null) armsAnimator.SetFloat(velocityHash, currentSpeed);
            if (bowAnimator != null) bowAnimator.SetFloat(velocityHash, currentSpeed);
        }
    }

    private void HandleChargeStart() => SetBool(isChargingHash, true);
    private void HandleChargeEnd() => SetBool(isChargingHash, false);
    private void HandleChargeCanceled() => SetTrigger(cancelChargeHash);
    private void HandleShoot() => SetTrigger(shootHash);
    private void HandleArrowChanged(ArrowType type)
    {
        ResetTrigger(cancelChargeHash);
        ResetTrigger(shootHash);
        SetTrigger(changeArrowHash);
    }

    private void SetBool(int hash, bool value)
    {
        if (armsAnimator != null) armsAnimator.SetBool(hash, value);
        if (bowAnimator != null) bowAnimator.SetBool(hash, value);
    }

    private void SetTrigger(int hash)
    {
        if (armsAnimator != null) armsAnimator.SetTrigger(hash);
        if (bowAnimator != null) bowAnimator.SetTrigger(hash);
    }

    private void ResetTrigger(int hash)
    {
        if (armsAnimator != null) armsAnimator.ResetTrigger(hash);
        if (bowAnimator != null) bowAnimator.ResetTrigger(hash);
    }
}
