using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayTransition(string title, string description, float displayDuration, bool autoFadeOut = true, Action onMidPoint = null, Action onComplete = null)
    {
        if (isTransitioning) return;
        _ = TransitionSequence(title, description, displayDuration, autoFadeOut, onMidPoint, onComplete);
    }


    public void PlayFadeOnly(float displayDuration, bool autoFadeOut = true, Action onMidPoint = null, Action onComplete = null)
    {
        if (isTransitioning) return;
        _ = TransitionSequence(null, null, displayDuration, autoFadeOut, onMidPoint, onComplete);
    }

    private async Task TransitionSequence(string title, string desc, float displayDuration, bool autoFadeOut, Action onMidPoint, Action onComplete)
    {
        isTransitioning = true;

        // Solo encendemos el texto si nos han pasado un texto válido
        if (titleText != null && !string.IsNullOrEmpty(title))
        {
            titleText.text = title;
            titleText.gameObject.SetActive(true);
        }
        if (descriptionText != null && !string.IsNullOrEmpty(desc))
        {
            descriptionText.text = desc;
            descriptionText.gameObject.SetActive(true);
        }

        if (backgroundPanel != null)
        {
            backgroundPanel.gameObject.SetActive(true);
        }

        SetAlpha(0f);
        
        await Fade(0f, 1f, fadeDuration);

        onMidPoint?.Invoke();

        await Task.Delay(Mathf.RoundToInt(displayDuration * 1000));

        if (autoFadeOut)
        {
            await Fade(1f, 0f, fadeDuration);
            if (backgroundPanel != null)
            {
                backgroundPanel.gameObject.SetActive(false);
            }

            if (titleText != null) titleText.gameObject.SetActive(false);
            if (descriptionText != null) descriptionText.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
        isTransitioning = false;
    }

    private async Task Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, timer / duration));
            await Task.Yield();
        }
        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (backgroundPanel != null)
        {
            Color c = backgroundPanel.color;
            c.a = alpha;
            backgroundPanel.color = c;
        }

        if (titleText != null && titleText.gameObject.activeSelf)
        {
            Color c = titleText.color;
            c.a = alpha;
            titleText.color = c;
        }
        if (descriptionText != null && descriptionText.gameObject.activeSelf)
        {
            Color c = descriptionText.color;
            c.a = alpha;
            descriptionText.color = c;
        }
    }
}