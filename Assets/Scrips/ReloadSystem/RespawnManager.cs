using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [SerializeField] private int objectsPerFrame = 25;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeReset = 4f;

    [Header("Death Phrases")]
    [SerializeField] private string[] deathPhrases = new string[7] 
    {
        "The village claims its tribute... but the debt is not yet paid.",
        "You cannot escape the altar forever.",
        "Death is no escape. The hunt begins anew.",
        "Your soul is bound to this place. Rise and suffer again.",
        "The sacrifice is only postponed.",
        "Run, little sacrifice. We enjoy the chase.",
        "The sickness of this land will not release you so easily."
    };

    private bool isRespawning;
    private readonly List<IResettable> resettables = new List<IResettable>();

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await Task.Yield();
        await FindAllResettables();
        await CaptureAllInitialStates();
    }

    private async Task FindAllResettables()
    {
        resettables.Clear();

        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        int count = 0;

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IResettable resettable)
            {
                resettables.Add(resettable);
            }

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }

    private async Task CaptureAllInitialStates()
    {
        int count = 0;

        foreach (IResettable resettable in resettables)
        {
            resettable.CaptureInitialState();

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }
    
    public async void ResetAllFromEvent()
    {
        if (isRespawning) return; 
        isRespawning = true;

        if (deathText != null)
        {
            deathText.gameObject.SetActive(false);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.unscaledDeltaTime / fadeDuration;
                SetFadeAlpha(alpha);
                await Task.Yield();
            }
            SetFadeAlpha(1f);
        }

        if (deathText != null)
        {
            if (deathPhrases != null && deathPhrases.Length > 0)
            {
                deathText.text = deathPhrases[Random.Range(0, deathPhrases.Length)];
            }
            deathText.gameObject.SetActive(true);
        }

        if (delayBeforeReset > 0f)
        {
            await Task.Delay(Mathf.RoundToInt(delayBeforeReset * 1000f));
        }

        if (deathText != null)
        {
            deathText.gameObject.SetActive(false);
        }

        await ResetAll();

        if (fadeImage != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.unscaledDeltaTime / fadeDuration;
                SetFadeAlpha(alpha);
                await Task.Yield();
            }
            SetFadeAlpha(0f);
            fadeImage.gameObject.SetActive(false);
        }

        isRespawning = false;
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }

    public async Task ResetAll()
    {
        int count = 0;

        foreach (IResettable resettable in resettables)
        {
            resettable.ResetState();

            count++;

            if (count >= objectsPerFrame)
            {
                count = 0;
                await Task.Yield();
            }
        }
    }
}