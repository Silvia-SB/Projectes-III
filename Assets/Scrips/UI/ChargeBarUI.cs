using UnityEngine;
using UnityEngine.UI;

public class ChargeBarUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerShooter playerShooter;

    [Header("UI References")]
    [SerializeField] private Slider chargeSlider;

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
        }
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart -= Show;
            playerShooter.OnChargeUpdate -= UpdateCharge;
            playerShooter.OnChargeEnd -= Hide;
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
    }

    public void Hide()
    {
        if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);
    }
}