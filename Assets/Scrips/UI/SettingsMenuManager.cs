using System;
using TMPro;
using UnityEngine;
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
    
    private Scene currentScene;
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private Image pauseMenuImage;
    
    private const string SensitivityKey = "MouseSensitivity";


    public void OnEnable()
    {
        currentScene = SceneManager.GetActiveScene();
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
    }
    
    public void CloseSettingsMenu()
    {
        if (currentScene.name == "MainMenu")
        {
            optionsImage.gameObject.SetActive(false);
            mainMenuImage.gameObject.SetActive(true);

        }
        else
        {
            optionsImage.gameObject.SetActive(false);
            pauseMenuImage.gameObject.SetActive(true);
        }
    }
}
