using UnityEngine;
using UnityEngine.UI;

public class ChargeBarUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerShooter playerShooter;

    [Header("UI References")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField] private Color chargingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;

    private void Awake()
    {
        if (chargeSlider == null)
            chargeSlider = GetComponent<Slider>();
            
        Hide();
    }

    private void OnEnable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart += Show;
            playerShooter.OnChargeUpdate += UpdateCharge;
            playerShooter.OnChargeEnd += Hide;
            playerShooter.OnMinChargeReached += HandleMinChargeReached;
        }
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart -= Show;
            playerShooter.OnChargeUpdate -= UpdateCharge;
            playerShooter.OnChargeEnd -= Hide;
            playerShooter.OnMinChargeReached -= HandleMinChargeReached;
        }
    }

    public void UpdateCharge(float currentChargeTime, float maxChargeTime)
    {
        if (chargeSlider != null)
        {
            if (!chargeSlider.gameObject.activeSelf)
                Show();
                
            chargeSlider.value = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        }
    }

    public void Show()
    {
        if (chargeSlider != null) chargeSlider.gameObject.SetActive(true);
        if (fillImage != null) fillImage.color = chargingColor;
    }

    private void HandleMinChargeReached()
    {
        if (fillImage != null) fillImage.color = readyColor;
    }

    public void Hide()
    {
        if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);
    }
}