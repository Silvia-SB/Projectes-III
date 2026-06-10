using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float delayBeforeLoad = 3f;
    
    [Header("Win Screen Content")]
    [SerializeField] private string winTitle = "YOU SURVIVED";
    [SerializeField, TextArea(2, 4)] private string winDescription = "The ancient gate opens to your salvation.";

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered || !other.CompareTag(playerTag)) return;

        hasBeenTriggered = true;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.PlayTransition(winTitle, winDescription, delayBeforeLoad, false,
                onMidPoint: () =>
                {
                    SceneManager.LoadScene(mainMenuSceneName);
                });
        }
    }
}