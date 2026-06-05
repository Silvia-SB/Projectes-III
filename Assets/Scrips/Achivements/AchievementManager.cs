using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AchievementData
{
    public string id;     
    public string title;    
    public string description; 
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Achivements database")]
    public List<AchievementData> achievements = new List<AchievementData>();

    public static event Action<AchievementData> OnAchievementUnlocked;

    private static HashSet<string> unlockedAchievements = new HashSet<string>();

    public static float lastKillTime;
    private static float kill_type = 3f;
    private static float general_kill = 5f;


    private static Dictionary<DamageType, List<float>> killTimesByType = new Dictionary<DamageType, List<float>>();
    private static List<float> allKillTimes = new List<float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        lastKillTime = Time.time; 
        killTimesByType.Clear();
        allKillTimes.Clear();

        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged += CheckSoulsAchievement;
        }
    }

    private void Update()
    {
        if (!unlockedAchievements.Contains("stop_running_away") && Time.time - lastKillTime >= 60f)
        {
            UnlockAchievement("stop_running_away");
        }
    }

    private void OnDestroy()
    {
        if (SoulManager.Instance != null)
        {
            SoulManager.Instance.OnSoulsChanged -= CheckSoulsAchievement;
        }
    }

    private void CheckSoulsAchievement(int currentSouls, int maxSouls)
    {
        if (maxSouls > 0 && currentSouls >= maxSouls)
        {
            UnlockAchievement("max_souls");
        }
    }

    public static void UnlockAchievement(string id)
    {
        if (Instance == null) return;

        if (!unlockedAchievements.Contains(id))
        {
            AchievementData data = Instance.achievements.Find(a => a.id == id);
            
            if (data != null)
            {
                unlockedAchievements.Add(id);
                Debug.Log("Logro Desbloqueado: " + data.title);
                OnAchievementUnlocked?.Invoke(data);
            }
            else
            {
                Debug.LogWarning($"Trying to get an achivement fail.ID doesnt exist: {id}");
            }
        }
    }

    public static void RecordKill(DamageType type)
    {
        lastKillTime = Time.time;
        
        if (Instance == null) return;

        //general registry for pentakill
        allKillTimes.Add(Time.time);
        allKillTimes.RemoveAll(t => Time.time - t > general_kill); 
        if (allKillTimes.Count >= 5)
        {
            UnlockAchievement("pentakill");
        }

        //registry for type multikill
        if (!killTimesByType.ContainsKey(type))
        {
            killTimesByType[type] = new List<float>();
        }

        killTimesByType[type].Add(Time.time);
        killTimesByType[type].RemoveAll(t => Time.time - t > kill_type);

        if (killTimesByType[type].Count >= 3)
        {
            switch (type)
            {
                case DamageType.Blood: UnlockAchievement("blood_multikill"); break;
                case DamageType.Electric: UnlockAchievement("electric_multikill"); break;
                case DamageType.Piercing: UnlockAchievement("piercing_multikill"); break;
            }
        }
    }
}