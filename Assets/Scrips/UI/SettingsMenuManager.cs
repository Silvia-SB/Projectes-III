using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class SettingsMenuManager : MonoBehaviour
{
    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    
    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("Sensibility Settings")]
    [SerializeField] private Slider sensitivitySlider;
    public static event Action<float> OnSensitivityChanged;

    [SerializeField] private GameObject titleLogo;
    

    
    private Scene currentScene;
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private Image pauseMenuImage;
    
    private const string SensitivityKey = "MouseSensitivity";


    public void OnEnable()
    {
        currentScene = SceneManager.GetActiveScene();
        Invoke(nameof(SelectMasterSlider), 0f);
    }

    public void FocusSettings()
    {
        Invoke(nameof(SelectMasterSlider), 0f);
    }
    
    
    private void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, sensitivitySlider.value);
        sensitivitySlider.value = savedSensitivity;

    }

    public void ChangeGraphicsQuality()
    {
        Debug.Log(graphicsDropdown.value);
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
    }
    public void ChangeMasterVolume()
    {
        audioMixer.SetFloat("MasterVolume", masterSlider.value);
    }
    public void ChangeSfxVolume()
    {
        audioMixer.SetFloat("SFXVolume", sfxSlider.value);
    }
    public void ChangeMusicVolume()
    {
        audioMixer.SetFloat("MusicVolume", musicSlider.value);
    }

    public void ChangeSensibility()
    {
        float value = sensitivitySlider.value;

        PlayerPrefs.SetFloat(SensitivityKey, value);
        PlayerPrefs.Save();
        if (currentScene.name == "SampleScene") OnSensitivityChanged?.Invoke(value);


    }

    public void CloseSettingsMenu()
    {
        if (currentScene.name == "MainMenu")
        {
            optionsImage.gameObject.SetActive(false);
            mainMenuImage.gameObject.SetActive(true);
            titleLogo.SetActive(true);
            SelectFirst(mainMenuImage.gameObject);
        }
        else
        {
            optionsImage.gameObject.SetActive(false);
            pauseMenuImage.gameObject.SetActive(true);
            SelectFirst(pauseMenuImage.gameObject);
        }
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

    private void SelectMasterSlider()
    {
        if (EventSystem.current == null || masterSlider == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(masterSlider.gameObject);
        masterSlider.Select();
    }
}
