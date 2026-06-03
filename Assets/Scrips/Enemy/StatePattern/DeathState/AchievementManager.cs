using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AchievementData
{
    public string id;     
    public string title;    
    public string description; 
    public Sprite icon;          
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achivements database")]
    public List<AchievementData> achievements = new List<AchievementData>();

    public static event Action<AchievementData> OnAchievementUnlocked;

    private static HashSet<string> unlockedAchievements = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static void UnlockAchievement(string id)
    {
        if (Instance == null) return;

        if (!unlockedAchievements.Contains(id))
        {
            AchievementData data = Instance.achievements.Find(a => a.id == id);
            unlockedAchievements.Add(id);
            Debug.Log("Logro Desbloqueado: " + data.title);
            OnAchievementUnlocked?.Invoke(data);
        }
    }
}