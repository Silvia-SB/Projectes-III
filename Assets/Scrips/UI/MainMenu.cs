using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private string loadingSceneName;

    [SerializeField] private Image controlsImage;
    
    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";

    private bool loadingScene;

    public void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, false);
        Invoke(nameof(SelectMainMenuFirst), 0f);
    }

    public async void PlayGame()
    {
        if (loadingScene) return;

        loadingScene = true;

        await LoadSceneAsync();
    }

    private async Task LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(loadingSceneName);

        while (!operation.isDone)
        {
            await Task.Yield();
        }
    }

    public void Options()
    {
        mainMenuImage.gameObject.SetActive(false);
        optionsImage.gameObject.SetActive(true);
        FocusSettings(optionsImage.gameObject);
    }

    public void Controls()
    {
        mainMenuImage.gameObject.SetActive(false);
        controlsImage.gameObject.SetActive(true);
        SelectFirst(controlsImage.gameObject);
    }

    public void BackFromControls()
    {
        controlsImage.gameObject.SetActive(false);
        mainMenuImage.gameObject.SetActive(true);
        SelectFirst(mainMenuImage.gameObject);
    }

    public void QuitGame()
    {

        Application.Quit();
    }

    private void SelectFirst(GameObject root)
    {
        if (root == null || EventSystem.current == null) return;

        Selectable[] selectables = root.GetComponentsInChildren<Selectable>(true);
        foreach (Selectable selectable in selectables)
        {
            if (!selectable.gameObject.activeInHierarchy || !selectable.IsInteractable()) continue;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            selectable.Select();
            return;
        }
    }

    private void SelectMainMenuFirst()
    {
        SelectFirst(mainMenuImage.gameObject);
    }

    private void FocusSettings(GameObject root)
    {
        SettingsMenuManager settingsMenuManager = root.GetComponentInChildren<SettingsMenuManager>(true);
        if (settingsMenuManager != null)
        {
            settingsMenuManager.FocusSettings();
            return;
        }

        SelectFirst(root);
    }
}
