using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private bool loadingScene;

    public void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
    }

    public void Controls()
    {
        titleLogo.SetActive(false);
        mainMenuImage.gameObject.SetActive(false);
        controlsImage.gameObject.SetActive(true);
    }

    public void BackFromControls()
    {
        controlsImage.gameObject.SetActive(false);
        mainMenuImage.gameObject.SetActive(true);
        titleLogo.SetActive(true);
    }

    public void QuitGame()
    {
        if (titleArrow != null)
            titleArrow.SalirDisparada();

        Application.Quit();
    }
}