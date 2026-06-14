using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class PlayerDeathController : MonoBehaviour, IResettable
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerLook look;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private HeadBobController headBob;
    [SerializeField] private Transform cameraTransform;

    [Header("Death Animation Settings")]
    [SerializeField] private float gravity = 15f;     
    [SerializeField] private float bounceFactor = 0.4f; 
    [SerializeField] private float tiltSpeed = 5f; 
    [SerializeField] private float tiltAngle = 75f; 
    [SerializeField] private float impactShakeIntensity = 0.6f; 

    [Header("Death Screen")]
    [SerializeField] private float delayBeforeScreen = 4.0f;
    [SerializeField] private float delayBeforeReset = 4f;
    [SerializeField] private string deathTitle = "YOU DIED";
    [SerializeField] private string[] deathPhrases = new string[]
    {
        "The village claims its tribute... but the debt is not yet paid.",
        "You cannot escape the altar forever.",
        "Death is no escape. The hunt begins anew.",
        "Your soul is bound to this place. Rise and suffer again.",
        "The sacrifice is only postponed.",
        "Run, little sacrifice. We enjoy the chase.",
        "The sickness of this land will not release you so easily."
    };
    private bool isDead = false;
    private Vector3 targetLocalPosition;
    private Quaternion targetLocalRotation;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 currentLerpPosition;
    private Quaternion currentLerpRotation;
    private float currentShake;
    private float velocityY;

    private void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<Health>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (look == null) look = GetComponent<PlayerLook>();
        if (shooter == null) shooter = GetComponent<PlayerShooter>();
        if (headBob == null) headBob = GetComponentInChildren<HeadBobController>();
        if (cameraTransform == null && headBob != null) cameraTransform = headBob.transform;

        if (cameraTransform != null)
        {
            initialLocalPosition = cameraTransform.localPosition;
            initialLocalRotation = cameraTransform.localRotation;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(HandleDeath);
            
            if (playerHealth is PlayerHealth ph)
                ph.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath.RemoveListener(HandleDeath);

            if (playerHealth is PlayerHealth ph)
                ph.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (isDead && currentHealth >= maxHealth)
        {
            isDead = false;
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = initialLocalPosition;
                cameraTransform.localRotation = initialLocalRotation;
            }
        }
    }

    private async void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        if (movement != null) movement.enabled = false;
        if (look != null) look.enabled = false;
        if (shooter != null) shooter.enabled = false;
        if (headBob != null) headBob.enabled = false;

        if (cameraTransform != null)
        {
            float distanceToGround = 1.5f; 
            if (Physics.Raycast(cameraTransform.position, Vector3.down, out RaycastHit hit, 5f, ~0, QueryTriggerInteraction.Ignore))
            {
                distanceToGround = hit.distance - 0.2f;
            }
            
            targetLocalPosition = new Vector3(cameraTransform.localPosition.x, cameraTransform.localPosition.y - distanceToGround, cameraTransform.localPosition.z);
            
            targetLocalRotation = cameraTransform.localRotation * Quaternion.Euler(25f, 0f, tiltAngle);

            currentLerpPosition = cameraTransform.localPosition;
            currentLerpRotation = cameraTransform.localRotation;
            currentShake = 0f;
            velocityY = 0f;
        }

        await Task.Delay(Mathf.RoundToInt(delayBeforeScreen * 1000));
        if (!isDead) return;

        if (TransitionManager.Instance != null)
        {
            string phrase = (deathPhrases != null && deathPhrases.Length > 0)
                ? deathPhrases[Random.Range(0, deathPhrases.Length)]
                : "The village consumed you...";

            TransitionManager.Instance.PlayTransition(deathTitle, phrase, delayBeforeReset, true,
                onMidPoint: () =>
                {
                    _ = RespawnManager.Instance?.ResetAll();
                });
        }
    }

    private void Update()
    {
        if (!isDead || cameraTransform == null) return;

        velocityY -= gravity * Time.deltaTime;
        currentLerpPosition.y += velocityY * Time.deltaTime;

        if (currentLerpPosition.y <= targetLocalPosition.y)
        {
            currentLerpPosition.y = targetLocalPosition.y;
            
            if (velocityY < -1f) 
            {
                currentShake = impactShakeIntensity * Mathf.Clamp01(Mathf.Abs(velocityY) / 5f); 
                velocityY = -velocityY * bounceFactor; 
            }
            else
            {
                velocityY = 0f; 
            }
        }

        currentLerpRotation = Quaternion.Slerp(currentLerpRotation, targetLocalRotation, Time.deltaTime * tiltSpeed);
        
        if (currentShake > 0f)
            currentShake = Mathf.Lerp(currentShake, 0f, Time.deltaTime * 3f);

        cameraTransform.localPosition = currentLerpPosition + Random.insideUnitSphere * currentShake;
        cameraTransform.localRotation = currentLerpRotation;
    }

    public void CaptureInitialState()
    {
        //Dont need to capture initial state
    }
    public void ResetState()
    { 
        playerHealth.enabled = true;
        movement.enabled = true;
        look.enabled = true;
        shooter.enabled = true;
        headBob.enabled = true;
        playerHealth.ClearStatuses();
        shooter.ResetState();
    }

    
}
