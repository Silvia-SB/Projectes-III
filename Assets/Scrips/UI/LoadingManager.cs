using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class LoadingManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName;

    [Header("UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text phraseText;

    [Header("Settings")]
    [SerializeField] private float visualSpeed = 0.7f;
    [SerializeField] private int minimumLoadingMilliseconds = 800;

    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";

    private readonly string[] loadingPhrases =
    {
        "Blood opens the gate...",
        "The dead remember...",
        "The village is watching...",
        "Your sins are loading...",
        "Ashes crawl beneath the stone..."
    };

    private async void Start()
    {
        if (phraseText != null)
            phraseText.text = loadingPhrases[Random.Range(0, loadingPhrases.Length)];

        await Task.Yield();
        await LoadGameSceneAsync();
    }

    private async Task LoadGameSceneAsync()
    {
        float visualProgress = 0f;
        float targetProgress = 0f;

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        UpdateText(0f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        operation.allowSceneActivation = false;

        float startTime = Time.realtimeSinceStartup;
        float minimumLoadingSeconds = minimumLoadingMilliseconds / 1000f;

        while (visualProgress < 1f)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / minimumLoadingSeconds);

            targetProgress = Mathf.Min(realProgress, timeProgress);

            visualProgress = Mathf.MoveTowards(
                visualProgress,
                targetProgress,
                visualSpeed * Time.unscaledDeltaTime
            );

            UpdateProgress(visualProgress);

            if (visualProgress >= 0.99f && operation.progress >= 0.9f)
                break;

            await Task.Yield();
        }

        UpdateProgress(1f);

        await Task.Delay(250);

        URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, true);

        operation.allowSceneActivation = true;
    }

    private void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        UpdateText(progress);
    }

    private void UpdateText(float progress)
    {
        if (progressText != null)
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}