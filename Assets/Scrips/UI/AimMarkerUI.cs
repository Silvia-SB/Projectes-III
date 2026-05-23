using UnityEngine;
using UnityEngine.UI;

public class AimMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private RectTransform crosshairRect;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float fadeSpeed = 15f;

    private Vector2 targetPosition;
    private bool isMarkerVisible;
    private Image crosshairImage;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    private void Awake()
    {
        if (crosshairRect == null)
        {
            CreateTemporaryCrosshair();
        }

        if (crosshairRect != null)
        {
            crosshairImage = crosshairRect.GetComponentInChildren<Image>(true);
            SetAlpha(0f);
            crosshairRect.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        if (crosshairImage != null)
        {
            Color c = crosshairImage.color;
            c.a = alpha;
            crosshairImage.color = c;
        }
    }

    private void CreateTemporaryCrosshair()
    {
        GameObject crosshairObj = new GameObject("GeneratedCrosshair");
        crosshairObj.transform.SetParent(transform, false);
        crosshairRect = crosshairObj.AddComponent<RectTransform>();

        Image img = crosshairObj.AddComponent<Image>();
        img.color = Color.red;
        crosshairRect.sizeDelta = new Vector2(10f, 10f);
    }

    private void OnEnable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeEnd += HideCrosshair;
            playerShooter.OnAimPointUpdated += UpdateCrosshairPosition;
            playerShooter.OnAimPointLost += HideCrosshair;
        }
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeEnd -= HideCrosshair;
            playerShooter.OnAimPointUpdated -= UpdateCrosshairPosition;
            playerShooter.OnAimPointLost -= HideCrosshair;
        }
    }

    private void ShowCrosshair()
    {
        if (crosshairRect != null && !isMarkerVisible)
        {
            crosshairRect.gameObject.SetActive(true);
            isMarkerVisible = true;
            targetAlpha = 1f;
        }
    }

    private void HideCrosshair()
    {
        if (crosshairRect != null && isMarkerVisible)
        {
            crosshairRect.gameObject.SetActive(false);
            isMarkerVisible = false;
            targetAlpha = 0f;
        }
    }

    private void UpdateCrosshairPosition(Vector2 screenPos)
    {
        if (crosshairRect != null)
        {
            if (!isMarkerVisible)
            {
                ShowCrosshair();
                if (currentAlpha < 0.05f)
                {
                    crosshairRect.position = screenPos; // La primera vez aparece directamente en el sitio
                }
            }
            
            targetPosition = screenPos;
        }
    }

    private void Update()
    {
        if (crosshairRect != null)
        {
            if (currentAlpha != targetAlpha)
            {
                SetAlpha(Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed));
                if (!isMarkerVisible && currentAlpha < 0.01f)
                {
                    SetAlpha(0f);
                    crosshairRect.gameObject.SetActive(false);
                }
            }

            if (isMarkerVisible && ((Vector2)crosshairRect.position - targetPosition).sqrMagnitude > 0.1f)
            {
                crosshairRect.position = Vector2.Lerp(crosshairRect.position, targetPosition, Time.deltaTime * smoothSpeed);
            }
        }
    }
}