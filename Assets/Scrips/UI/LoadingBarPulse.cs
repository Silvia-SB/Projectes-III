using UnityEngine;
using UnityEngine.UI;

public class LoadingBarPulse : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.85f;
    [SerializeField] private float maxAlpha = 1f;

    private Color originalColor;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        originalColor = targetImage.color;
    }

    private void Update()
    {
        float pulse = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f
        );

        Color color = originalColor;
        color.a = pulse;
        targetImage.color = color;
    }
}