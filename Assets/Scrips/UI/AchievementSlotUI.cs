using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image backgroundImage; 
    
    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color unlockedColor = new Color(1f, 1f, 1f, 1f);

    public void Setup(AchievementData data, bool isUnlocked)
    {
        if (titleText != null) titleText.text = data.title;
        
        if (descriptionText != null) descriptionText.text = isUnlocked ? data.description : "???";

        if (backgroundImage != null)
        {
            backgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }
}