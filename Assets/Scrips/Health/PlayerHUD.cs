using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float healthLerpSpeed = 5f; // Velocidad de la animación de la barra

    [Header("Souls UI")]
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private float soulsLerpSpeed = 10f; // Velocidad de la animación de las almas

    private float targetHealthValue;
    private float currentDisplayedSouls;
    private int targetSoulsValue;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
            
            // Inicializar directamente (Snap) para evitar desincronizaciones al cargar la escena
            if (playerHealth.MaxHealth > 0 && healthBar != null)
            {
                targetHealthValue = playerHealth.CurrentHealth / playerHealth.MaxHealth;
                healthBar.value = targetHealthValue;
            }
        }
    }

    private void Start()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged += UpdateSoulsUI;
            
            // Inicializar el UI con los valores exactos (Snap) al inicio para evitar animaciones largas
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
        // Always unsubscribe from events to prevent memory leaks and null reference exceptions!
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
        if (healthBar != null && maxHealth > 0)
        {
            targetHealthValue = currentHealth / maxHealth;
        }
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
