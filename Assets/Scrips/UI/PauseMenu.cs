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
        if (musicMenu != null)
        {
            musicMenu.SetActive(true);
            SetMusicMenuUnpaused();
        }
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
        FocusSettings(settingsMenu.gameObject);
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

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            selectable.Select();
            return;
        }
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
    
    private void SetMusicMenuUnpaused()
    {
        if (musicMenu == null) return;

        AudioSource[] audioSources = musicMenu.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.ignoreListenerPause = true;
        }

        Animator[] animators = musicMenu.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
    
}
