using UnityEngine;
using System.Collections.Generic;

public class AchievementMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject achievementSlotPrefab; 
    [SerializeField] private Transform contentParent; 

    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void OnEnable()
    {
        RefreshMenu(); 
    }

    public void RefreshMenu()
    {
        if (AchievementManager.Instance == null) return;

        // 1. Limpiamos la lista vieja (por si abres y cierras el menú varias veces)
        foreach (var slot in spawnedSlots) Destroy(slot);
        spawnedSlots.Clear();

        // 2. Generamos la lista nueva
        foreach (var achievement in AchievementManager.Instance.achievements)
        {
            GameObject newSlot = Instantiate(achievementSlotPrefab, contentParent);
            spawnedSlots.Add(newSlot);

            AchievementSlotUI slotUI = newSlot.GetComponent<AchievementSlotUI>();
            if (slotUI != null)
            {
                bool isUnlocked = AchievementManager.IsAchievementUnlocked(achievement.id);
                slotUI.Setup(achievement, isUnlocked);
            }
        }
    }
}