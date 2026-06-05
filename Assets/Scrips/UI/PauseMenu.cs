using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Image settingsMenu;
    [SerializeField] private Image pauseMenu;
    [SerializeField] private GameObject achievementsMenu;
    
    
    public void PauseGame()
    {
        if (settingsMenu.gameObject.activeSelf)
        {
            settingsMenu.gameObject.SetActive(false);
        }
        if (achievementsMenu != null && achievementsMenu.activeSelf)
        {
            achievementsMenu.SetActive(false);
        }
        if (pauseMenu.gameObject.activeSelf)
        {
            ResumeGame();
            return;
        }
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.gameObject.SetActive(false);
    }
    
    public void Options()
    {
        pauseMenu.gameObject.SetActive(false);
        settingsMenu.gameObject.SetActive(true);
    }

    public void Achievements()
    {
        pauseMenu.gameObject.SetActive(false);
        if (achievementsMenu != null) achievementsMenu.SetActive(true);
    }

    public void CloseAchievements()
    {
        if (achievementsMenu != null) achievementsMenu.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
}
