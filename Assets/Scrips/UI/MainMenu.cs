using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private string playSceneName;

    [Header("Title Arrow")]
    [SerializeField] private TitleArrowShot titleArrow;
    [SerializeField] private float delayAfterArrowExit = 0.35f;

    [SerializeField] private GameObject titleLogo;
    [SerializeField] private Image controlsImage;

    private bool loadingScene;
    private float loadTimer;

    public void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!loadingScene) return;

        loadTimer += Time.deltaTime;

        if (loadTimer >= delayAfterArrowExit)
        {
            SceneManager.LoadScene(playSceneName);
        }
    }

    public void PlayGame()
    {
        titleArrow.SalirDisparada();

        loadingScene = true;
        loadTimer = 0f;
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
        titleArrow.SalirDisparada();
        Application.Quit();
    }
}