using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (AchievementManager.Instance == null)
        {
            Debug.Log("AchievementManager is null"); 
            return;
        }

        foreach (var slot in spawnedSlots) Destroy(slot);
        spawnedSlots.Clear();

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

        SelectFirst();
    }

    private void SelectFirst()
    {
        if (EventSystem.current == null) return;

        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
        foreach (Selectable selectable in selectables)
        {
            if (!selectable.gameObject.activeInHierarchy || !selectable.IsInteractable()) continue;

            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            selectable.Select();
            return;
        }
    }
}
