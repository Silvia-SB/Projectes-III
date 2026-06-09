using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class LevelIntroManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private Image imageToFade;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayDuration = 6f; 

    [Header("Text Content")]
    [SerializeField] private string titleString = "Echoes of the Accursed";
    [SerializeField, TextArea(3, 6)] 
    private string descriptionString = "You awaken in a village consumed by madness and plague. They hunger for your sacrifice, but you must flee. Seek out the three iron bells hidden within these cursed grounds and strike them with your arrows. Only when the third bell tolls will the ancient mechanism awaken, opening the great gate to your salvation.";

    private async void Start()
    {
        await ShowIntroSequence();
    }

    private async Task ShowIntroSequence()
    {
        if (titleText != null) titleText.text = titleString;
        if (descriptionText != null) descriptionText.text = descriptionString;

        Time.timeScale = 0f;
        
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            SetAlpha(1f);
        }

        await Task.Delay(Mathf.RoundToInt(displayDuration * 1000));

        if (introPanel != null)
        {
            float fadeTimer = 0f;
            while (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration));
                await Task.Yield();
            }
            introPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void SetAlpha(float alpha)
    {
        if (imageToFade != null)
        {
            Color c = imageToFade.color;
            c.a = alpha;
            imageToFade.color = c;
        }
        
        if (titleText != null)
        {
            Color c = titleText.color;
            c.a = alpha;
            titleText.color = c;
        }
        
        if (descriptionText != null)
        {
            Color c = descriptionText.color;
            c.a = alpha;
            descriptionText.color = c;
        }
    }
}