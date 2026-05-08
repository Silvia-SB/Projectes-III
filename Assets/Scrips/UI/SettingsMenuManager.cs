using TMPro;
using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    public TMP_Dropdown graphisDropdown;
    
    public void ChangeGraphicsQuality()
    {
        Debug.Log(graphisDropdown.value);
        QualitySettings.SetQualityLevel(graphisDropdown.value);
    }
}
