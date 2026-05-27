using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}