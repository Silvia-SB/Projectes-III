using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private string playSceneName;

    public void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(playSceneName);
    }
    public void Options()
    {
        mainMenuImage.gameObject.SetActive(false);
        optionsImage.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
