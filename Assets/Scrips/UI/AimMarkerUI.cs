using UnityEngine;
using UnityEngine.UI;

public class AimMarkerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShooter playerShooter;
    [SerializeField] private RectTransform crosshairRect;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 20f;

    private Vector2 targetPosition;
    private bool isMarkerVisible;

    private void Awake()
    {
        if (crosshairRect == null)
        {
            CreateTemporaryCrosshair();
        }

        if (crosshairRect != null)
        {
            crosshairRect.gameObject.SetActive(false);
        }
    }

    private void CreateTemporaryCrosshair()
    {
        GameObject crosshairObj = new GameObject("GeneratedCrosshair");
        crosshairObj.transform.SetParent(transform, false);
        crosshairRect = crosshairObj.AddComponent<RectTransform>();

        GameObject hLine = new GameObject("HLine");
        hLine.transform.SetParent(crosshairObj.transform, false);
        Image hImg = hLine.AddComponent<Image>();
        hImg.color = Color.red;
        hLine.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 4f);

        GameObject vLine = new GameObject("VLine");
        vLine.transform.SetParent(crosshairObj.transform, false);
        Image vImg = vLine.AddComponent<Image>();
        vImg.color = Color.red;
        vLine.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 20f);
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
        }
    }

    private void HideCrosshair()
    {
        if (crosshairRect != null && isMarkerVisible)
        {
            crosshairRect.gameObject.SetActive(false);
            isMarkerVisible = false;
        }
    }

    private void UpdateCrosshairPosition(Vector2 screenPos)
    {
        if (crosshairRect != null)
        {
            if (!isMarkerVisible)
            {
                ShowCrosshair();
                crosshairRect.position = screenPos; // La primera vez aparece directamente en el sitio
            }
            
            targetPosition = screenPos;
        }
    }

    private void Update()
    {
        if (isMarkerVisible && crosshairRect != null)
        {
            if (((Vector2)crosshairRect.position - targetPosition).sqrMagnitude > 0.1f)
            {
                crosshairRect.position = Vector2.Lerp(crosshairRect.position, targetPosition, Time.deltaTime * smoothSpeed);
            }
        }
    }
}