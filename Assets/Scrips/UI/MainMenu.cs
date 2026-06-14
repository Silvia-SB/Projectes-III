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

    [Header("Title Arrow")]
    [SerializeField] private TitleArrowShot titleArrow;
    [SerializeField] private float delayAfterArrowExit = 0.35f;

    [SerializeField] private GameObject titleLogo;
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

        if (titleArrow != null)
            titleArrow.SalirDisparada();

        await Task.Delay(Mathf.RoundToInt(delayAfterArrowExit * 1000f));

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
        titleLogo.SetActive(false);

        mainMenuImage.gameObject.SetActive(false);
        optionsImage.gameObject.SetActive(true);
        FocusSettings(optionsImage.gameObject);
    }

    public void Controls()
    {
        titleLogo.SetActive(false);
        mainMenuImage.gameObject.SetActive(false);
        controlsImage.gameObject.SetActive(true);
        SelectFirst(controlsImage.gameObject);
    }

    public void BackFromControls()
    {
        controlsImage.gameObject.SetActive(false);
        mainMenuImage.gameObject.SetActive(true);
        titleLogo.SetActive(true);
        SelectFirst(mainMenuImage.gameObject);
    }

    public void QuitGame()
    {
        if (titleArrow != null)
            titleArrow.SalirDisparada();

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
