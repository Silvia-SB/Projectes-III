using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Image settingsMenu;
    [SerializeField] private Image pauseMenu;
    [SerializeField] private GameObject achievementsMenu;
    [SerializeField] private AudioManagerEnemyZone audioManagerEnemyZone;
    [SerializeField] private GameObject musicMenu;
    
    
    public void PauseGame()
    {
        if (musicMenu != null) musicMenu.SetActive(true);
        audioManagerEnemyZone?.PauseAudio();
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
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.gameObject.SetActive(true);
        SelectFirst(pauseMenu.gameObject);
    }

    public void ResumeGame()
    {
        if (musicMenu != null) musicMenu.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.gameObject.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
        audioManagerEnemyZone?.ResumeAudio();
    }
    
    public void Options()
    {
        pauseMenu.gameObject.SetActive(false);
        settingsMenu.gameObject.SetActive(true);
        SelectFirst(settingsMenu.gameObject);
    }

    public void Achievements()
    {
        pauseMenu.gameObject.SetActive(false);
        if (achievementsMenu != null) achievementsMenu.SetActive(true);
        SelectFirst(achievementsMenu);
    }

    public void CloseAchievements()
    {
        if (achievementsMenu != null) achievementsMenu.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
        SelectFirst(pauseMenu.gameObject);
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

            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            selectable.Select();
            return;
        }
    }
    
}
