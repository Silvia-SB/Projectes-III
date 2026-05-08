using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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
}
