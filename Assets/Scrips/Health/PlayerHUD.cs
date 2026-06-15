using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float healthLerpSpeed = 5f;

    [Header("Souls UI")]
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private float soulsLerpSpeed = 10f;

    [Header("Arrow Unlock UI")]
    [SerializeField] private Image bloodArrowImage;
    [SerializeField] private Image piercingArrowImage;
    [SerializeField] private Image electricArrowImage;

    [Header("Damage Visuals")]
    [SerializeField] private Image lowHealthVignette;
    [SerializeField] private float lowHealthThreshold = 0.5f; 
    [SerializeField] private float blinkThreshold = 0.15f; 
    [SerializeField] private float blinkSpeed = 10f;
    [SerializeField] private Image hitFlashImage;
    [SerializeField] private float hitFlashFadeSpeed = 3f;
    [SerializeField, Range(0f, 1f)] private float hitFlashMaxAlpha = 1f;

    [Header("Status Visuals")]
    [SerializeField] private Image fireOverlayImage;
    [SerializeField] private Image fireOverlayImage2;
    [SerializeField] private Image electricOverlayImage;
    [SerializeField] private float electricFlashSpeed = 15f;
    [SerializeField] private float fireFlashFadeSpeed = 5f;
    [SerializeField, Range(0f, 1f)] private float fireFlashMaxAlpha = 0.8f;
    [SerializeField, Range(0f, 1f)] private float fireBaseAlpha = 0.3f;
    [SerializeField, Range(0f, 1f)] private float electricBaseAlpha = 0.4f;

    private float targetHealthValue;
    private float currentDisplayedSouls;
    private int targetSoulsValue;
    private float extraVignetteAlpha;

    private StatusEffectManager playerStatusManager;
    private float fireCurrentAlpha;
    private bool isBurning;
    private bool isElectrocuted;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            
            playerStatusManager = playerHealth.GetComponent<StatusEffectManager>();
            if (playerStatusManager != null)
            {
                playerStatusManager.OnStatusApplied += OnStatusApplied;
                playerStatusManager.OnStatusRemoved += OnStatusRemoved;
                playerStatusManager.OnStatusTick += OnStatusTick;
                playerStatusManager.OnAllStatusesCleared += OnAllStatusesCleared;
            }
            
            if (playerHealth.MaxHealth > 0)
            {
                targetHealthValue = playerHealth.CurrentHealth / playerHealth.MaxHealth;
                if (healthBar != null) healthBar.value = targetHealthValue;
            }
        }

        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
            c.a = 0f;
            hitFlashImage.color = c;
        }
        if (lowHealthVignette != null)
        {
            Color c = lowHealthVignette.color;
            c.a = 0f;
            lowHealthVignette.color = c;
        }
        
        if (fireOverlayImage != null)
        {
            Color c = fireOverlayImage.color;
            c.a = 0f;
            fireOverlayImage.color = c;
        }
        if (fireOverlayImage2 != null)
        {
            Color c = fireOverlayImage2.color;
            c.a = 0f;
            fireOverlayImage2.color = c;
        }
        if (electricOverlayImage != null)
        {
            Color c = electricOverlayImage.color;
            c.a = 0f;
            electricOverlayImage.color = c;
        }
    }

    private void Start()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged += UpdateSoulsUI;
            
            targetSoulsValue = SoulManager.Instance.CurrentSouls;
            currentDisplayedSouls = targetSoulsValue;
            if (soulsText != null)
            {
                soulsText.text = $"Souls: {targetSoulsValue}";
            }
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
        
        if (playerStatusManager != null)
        {
            playerStatusManager.OnStatusApplied -= OnStatusApplied;
            playerStatusManager.OnStatusRemoved -= OnStatusRemoved;
            playerStatusManager.OnStatusTick -= OnStatusTick;
            playerStatusManager.OnAllStatusesCleared -= OnAllStatusesCleared;
        }

        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged -= UpdateSoulsUI;
        }

        isBurning = false;
        isElectrocuted = false;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (maxHealth > 0)
        {
            float newTarget = currentHealth / maxHealth;
            
            if (newTarget < targetHealthValue)
            {
                TriggerHitFlash();
            }
            
            targetHealthValue = newTarget;
        }
    }

    private void TriggerHitFlash()
    {
        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
            c.a = hitFlashMaxAlpha;
            hitFlashImage.color = c;
        }
        extraVignetteAlpha = 0.6f; 
    }

    private void Update()
    {
        if (healthBar != null && healthBar.value != targetHealthValue)
        {
            healthBar.value = Mathf.Lerp(healthBar.value, targetHealthValue, Time.deltaTime * healthLerpSpeed);
            
            if (Mathf.Abs(healthBar.value - targetHealthValue) <= 0.001f)
            {
                healthBar.value = targetHealthValue;
            }
        }

        
        if (hitFlashImage != null && hitFlashImage.color.a > 0)
        {
            Color c = hitFlashImage.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * hitFlashFadeSpeed);
            hitFlashImage.color = c;
        }

        if (extraVignetteAlpha > 0f)
        {
            extraVignetteAlpha = Mathf.MoveTowards(extraVignetteAlpha, 0f, Time.deltaTime * hitFlashFadeSpeed);
        }

        if (lowHealthVignette != null)
        {
            float alpha = 0f;
            if (targetHealthValue <= lowHealthThreshold)
            {
                alpha = (lowHealthThreshold - targetHealthValue) / lowHealthThreshold;  
                
                if (targetHealthValue <= blinkThreshold)
                {
                    float blink = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
                    alpha *= Mathf.Lerp(0.4f, 1f, blink);
                }
            }
            
            alpha = Mathf.Clamp01(alpha + extraVignetteAlpha);

            Color vc = lowHealthVignette.color;
            vc.a = Mathf.Lerp(vc.a, alpha, Time.deltaTime * 5f);
            lowHealthVignette.color = vc;
        }

        if (soulsText != null && currentDisplayedSouls != targetSoulsValue)
        {
            currentDisplayedSouls = Mathf.Lerp(currentDisplayedSouls, targetSoulsValue, Time.deltaTime * soulsLerpSpeed);
            
            if (Mathf.Abs(currentDisplayedSouls - targetSoulsValue) <= 0.5f)
            {
                currentDisplayedSouls = targetSoulsValue;
            }

            soulsText.text = $"Souls: {Mathf.RoundToInt(currentDisplayedSouls)}";
        }
        
        UpdateArrowFillUI();
        UpdateStatusOverlays();
    }

    private void UpdateStatusOverlays()
    {
        if (fireOverlayImage != null || fireOverlayImage2 != null)
        {
            if (isBurning)
            {
                fireCurrentAlpha = Mathf.MoveTowards(fireCurrentAlpha, fireBaseAlpha, Time.deltaTime * fireFlashFadeSpeed);
            }
            else if (fireCurrentAlpha > 0f)
            {
                fireCurrentAlpha = Mathf.MoveTowards(fireCurrentAlpha, 0f, Time.deltaTime * fireFlashFadeSpeed);
            }
            
            if (fireOverlayImage != null)
            {
                Color c = fireOverlayImage.color;
                c.a = fireCurrentAlpha;
                fireOverlayImage.color = c;
            }
            
            if (fireOverlayImage2 != null)
            {
                Color c = fireOverlayImage2.color;
                c.a = fireCurrentAlpha;
                fireOverlayImage2.color = c;
            }
        }

        if (electricOverlayImage != null)
        {
            if (isElectrocuted)
            {
                float targetAlpha = electricBaseAlpha + (Mathf.Abs(Mathf.Sin(Time.time * electricFlashSpeed)) * (1f - electricBaseAlpha));
                Color c = electricOverlayImage.color;
                c.a = targetAlpha;
                electricOverlayImage.color = c;
            }
            else if (electricOverlayImage.color.a > 0f)
            {
                Color c = electricOverlayImage.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * fireFlashFadeSpeed);
                electricOverlayImage.color = c;
            }
        }
    }

    private void UpdateArrowFillUI()
    {
        if (SoulManager.Instance == null) return;

        if (bloodArrowImage != null)
        {
            float cost = SoulManager.Instance.GetArrowCost(ArrowType.Blood);
            bloodArrowImage.fillAmount = cost > 0 ? Mathf.Clamp01(currentDisplayedSouls / cost) : 1f;
        }
        if (piercingArrowImage != null)
        {
            float cost = SoulManager.Instance.GetArrowCost(ArrowType.Piercing);
            piercingArrowImage.fillAmount = cost > 0 ? Mathf.Clamp01(currentDisplayedSouls / cost) : 1f;
        }
        if (electricArrowImage != null)
        {
            float cost = SoulManager.Instance.GetArrowCost(ArrowType.Electric);
            electricArrowImage.fillAmount = cost > 0 ? Mathf.Clamp01(currentDisplayedSouls / cost) : 1f;
        }
    }

    private void UpdateSoulsUI(int currentSouls, int maxSouls)
    {
        targetSoulsValue = currentSouls;
    }

    private void OnStatusApplied(DamageType type)
    {
        if (type == DamageType.Blood)
        {
            isBurning = true;
            fireCurrentAlpha = fireFlashMaxAlpha; 
        }
        else if (type == DamageType.Electric)
        {
            isElectrocuted = true;
        }
    }

    private void OnStatusRemoved(DamageType type)
    {
        if (type == DamageType.Blood)
        {
            isBurning = false;
        }
        else if (type == DamageType.Electric)
        {
            isElectrocuted = false;
        }
    }

    private void OnStatusTick(DamageType type)
    {
        if (type == DamageType.Blood && isBurning)
        {
            fireCurrentAlpha = fireFlashMaxAlpha; 
        }
    }

    private void OnAllStatusesCleared()
    {
        OnStatusRemoved(DamageType.Blood);
        OnStatusRemoved(DamageType.Electric);
    }
}
