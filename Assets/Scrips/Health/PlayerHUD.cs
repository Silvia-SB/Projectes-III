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

    [Header("Damage Visuals")]
    [SerializeField] private Image lowHealthVignette;
    [SerializeField] private float lowHealthThreshold = 0.5f; 
    [SerializeField] private float blinkThreshold = 0.15f; 
    [SerializeField] private float blinkSpeed = 10f;
    [SerializeField] private Image hitFlashImage;
    [SerializeField] private float hitFlashFadeSpeed = 3f;

    private float targetHealthValue;
    private float currentDisplayedSouls;
    private int targetSoulsValue;
    private float extraVignetteAlpha;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            
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

        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged -= UpdateSoulsUI;
        }
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
            c.a = 1f;
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
                alpha = (lowHealthThreshold - targetHealthValue) / lowHealthThreshold; // De 0 a 1
                
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
    }

    private void UpdateSoulsUI(int currentSouls, int maxSouls)
    {
        targetSoulsValue = currentSouls;
    }
}
