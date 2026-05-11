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
    public static event Action <float> OnSensibilityChanged;
    
    private Scene currentScene;
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Image optionsImage;
    [SerializeField] private Image mainMenuImage;
    [SerializeField] private Image pauseMenuImage;
    

    public void OnEnable()
    {
        currentScene = SceneManager.GetActiveScene();
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
        OnSensibilityChanged?.Invoke(sensitivitySlider.value);
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
