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

    [Header("Settings")]
    [SerializeField] private float visualSpeed = 0.7f;
    [SerializeField] private int minimumLoadingMilliseconds = 800;
    
    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";

    private async void Start()
    {
        await Task.Yield();
        await LoadGameSceneAsync();
    }

    private async Task LoadGameSceneAsync()
    {
        float visualProgress = 0f;
        float targetProgress = 0f;

        if (progressBar != null)
            progressBar.value = 0f;

        UpdateText(0f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        operation.allowSceneActivation = false;

        float startTime = Time.realtimeSinceStartup;

        while (operation.progress < 0.9f)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            visualProgress = Mathf.MoveTowards(
                visualProgress,
                targetProgress,
                visualSpeed * Time.unscaledDeltaTime
            );

            UpdateProgress(visualProgress);

            await Task.Yield();
        }

        targetProgress = 0.95f;

        while (visualProgress < targetProgress)
        {
            visualProgress = Mathf.MoveTowards(
                visualProgress,
                targetProgress,
                visualSpeed * Time.unscaledDeltaTime
            );

            UpdateProgress(visualProgress);

            await Task.Yield();
        }

        while ((Time.realtimeSinceStartup - startTime) * 1000f < minimumLoadingMilliseconds)
        {
            await Task.Yield();
        }

        UpdateProgress(0.98f);

        await Task.Yield();
        
        SetRendererFeatureActive(fullScreenFeatureName, true);

        operation.allowSceneActivation = true;
    }
    
    private void SetRendererFeatureActive(string targetFeatureName, bool active)
    {
        if (pcRendererData == null)
        {
            Debug.LogWarning("PC Renderer Data no asignado.");
            return;
        }

        foreach (ScriptableRendererFeature feature in pcRendererData.rendererFeatures)
        {
            if (feature == null) continue;

            if (feature.name == targetFeatureName)
            {
                feature.SetActive(active);
                pcRendererData.SetDirty();
                return;
            }
        }

        Debug.LogWarning("No se encontró el Renderer Feature: " + targetFeatureName);
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