using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuMusic : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

    private static MainMenuMusic instance;
    private AudioSource[] audioSources;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSources = GetComponentsInChildren<AudioSource>(true);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateActiveState(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateActiveState(scene);
    }

    private void UpdateActiveState(Scene scene)
    {
        bool shouldPlay = !string.Equals(scene.name, GameplaySceneName, System.StringComparison.OrdinalIgnoreCase);

        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == null) continue;

            audioSource.enabled = shouldPlay;

            if (shouldPlay)
            {
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }
}
