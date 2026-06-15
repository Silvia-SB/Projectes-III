using UnityEngine;

public class LevelIntroManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 6f;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerAudio playerAudio;

    [Header("Text Content")]
    [SerializeField] private string titleString = "Echoes of the Accursed";
    [SerializeField, TextArea(3, 6)] 
    private string descriptionString = "You awaken in a village consumed by madness and plague. They hunger for your sacrifice, but you must flee. Seek out the three iron bells hidden within these cursed grounds and strike them with your arrows. Only when the third bell tolls will the ancient mechanism awaken, opening the great gate to your salvation.";

    private void Awake()
    {
        gameObject.SetActive(true);
    }
    private void Start()
    {
        if (TransitionManager.Instance != null)
        {
            SetActivePlayer(false);
            
            TransitionManager.Instance.PlayTransition(titleString, descriptionString, displayDuration, true, 
                onComplete: () => {
                    SetActivePlayer(true);
                    AchievementManager.UnlockAchievement("level_start");
                },
                startFullyVisible: true);
        }
    }

    private void SetActivePlayer(bool isActive)
    {
        playerMovement.enabled = isActive;
        playerLook.enabled = isActive;
        playerAudio.enabled = isActive;
    }
}
