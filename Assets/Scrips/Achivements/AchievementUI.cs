using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Necesario para usar Queue (Colas)

public class AchievementUI : MonoBehaviour
{
    private enum UIState { Idle, ScalingIn, Displaying, ScalingOut }

    [Header("References")]
    [SerializeField] private GameObject achievementPanel; 
    [SerializeField] private TextMeshProUGUI titleText; 
    [SerializeField] private TextMeshProUGUI descriptionText; 
    [SerializeField] private Image iconImage; 
    
    [Header("Configuration")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float scaleDuration = 0.3f; 

    private float displayTimer = 0f;
    private float scaleTimer = 0f;
    private UIState currentState = UIState.Idle;

    private Queue<AchievementData> achievementQueue = new Queue<AchievementData>();

    private void OnEnable()
    {
        AchievementManager.OnAchievementUnlocked += ShowAchievement;
    }

    private void OnDisable()
    {
        AchievementManager.OnAchievementUnlocked -= ShowAchievement;
    }

    private void Start()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }

    private void ShowAchievement(AchievementData data)
    {
        achievementQueue.Enqueue(data);
    }

    private void StartDisplaying(AchievementData data)
    {
        if (achievementPanel == null) return;

        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;

        achievementPanel.SetActive(true);
        achievementPanel.transform.localScale = Vector3.zero; 
        
        scaleTimer = 0f;
        currentState = UIState.ScalingIn; 
    }

    private void Update()
    {
        if (currentState == UIState.Idle)
        {
            if (achievementQueue.Count > 0)
            {
                AchievementData nextAchievement = achievementQueue.Dequeue();
                StartDisplaying(nextAchievement);
            }
            return;
        }

        if (currentState == UIState.ScalingIn)
        {
            scaleTimer += Time.deltaTime;
            float t = Mathf.Clamp01(scaleTimer / scaleDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            achievementPanel.transform.localScale = Vector3.one * smoothT;

            if (t >= 1f)
            {
                currentState = UIState.Displaying;
                displayTimer = displayDuration;
            }
        }
        else if (currentState == UIState.Displaying)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f)
            {
                currentState = UIState.ScalingOut;
                scaleTimer = 0f;
            }
        }
        else if (currentState == UIState.ScalingOut)
        {
            scaleTimer += Time.deltaTime;
            float t = Mathf.Clamp01(scaleTimer / scaleDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            achievementPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, smoothT);

            if (t >= 1f)
            {
                achievementPanel.SetActive(false);
                currentState = UIState.Idle;
            }
        }
    }
}