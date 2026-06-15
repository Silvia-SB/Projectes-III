using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AchievementMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject achievementSlotPrefab; 
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Selectable firstSelected;

    [Header("Controller")]
    [SerializeField] private float controllerScrollSpeed = 1.5f;

    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void OnEnable()
    {
        ResolveReferences();
        RefreshMenu(); 
    }

    private void Update()
    {
        ScrollWithController();
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

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        SelectFirst();
    }

    public void FocusAchievements()
    {
        SelectFirst();
    }

    private void SelectFirst()
    {
        if (EventSystem.current == null) return;

        if (firstSelected != null && firstSelected.gameObject.activeInHierarchy && firstSelected.IsInteractable())
        {
            Select(firstSelected);
            return;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (!button.gameObject.activeInHierarchy || !button.IsInteractable()) continue;

            Select(button);
            return;
        }

        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
        foreach (Selectable selectable in selectables)
        {
            if (!selectable.gameObject.activeInHierarchy || !selectable.IsInteractable()) continue;
            if (selectable is Scrollbar) continue;

            Select(selectable);
            return;
        }
    }

    private void Select(Selectable selectable)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        selectable.Select();
    }

    private void ScrollWithController()
    {
        if (scrollRect == null) return;

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        float input = gamepad.leftStick.ReadValue().y + gamepad.dpad.ReadValue().y;
        if (Mathf.Abs(input) < 0.2f) return;

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            scrollRect.verticalNormalizedPosition + input * controllerScrollSpeed * Time.unscaledDeltaTime);
    }

    private void ResolveReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }
    }
}
