using UnityEngine;
using static AudioProfile;

public class AudioSFXManager : MonoBehaviour
{
    public static AudioSFXManager Instance { get; private set; }

    [Header("SFX Pool")]
    [SerializeField] private int poolSize = 30;

    [Header("3D Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 25f;

    private AudioSource[] sfxPool;
    private int index;

    private void Awake()
    {
        Instance = this;
        CreatePool();
    }

    private void CreatePool()
    {
        sfxPool = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObject = new GameObject("SFX_Source_" + i);
            audioObject.transform.SetParent(transform);

            AudioSource source = audioObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;

            sfxPool[i] = source;
        }
    }

    public static void PlaySFX(Collider collider, Vector3 position, ArrowType arrowType, bool isFullyCharged)
    {
        if (Instance == null) return;

        AudioProfileReference profileRef = collider.GetComponent<AudioProfileReference>();

        if (profileRef == null)
            profileRef = collider.GetComponentInParent<AudioProfileReference>();

        if (profileRef == null) return;

        AudioConfig audioConfig = profileRef.GetClipFromProfile(arrowType, isFullyCharged);

        Instance.PlaySFXAtPosition(audioConfig, position, audioConfig.volume);
    }
    
    private void PlaySFXAtPosition(AudioConfig audioConfig, Vector3 position, float volume)
    {
        if (audioConfig.clip == null) return;

        AudioSource source = GetSource();

        source.transform.position = position;
        source.volume = volume;
        source.pitch = 1f;
        source.spatialBlend = 1f;
        source.clip = audioConfig.clip;
        source.time = audioConfig.startTime;

        source.Play();

        if (audioConfig.endTime > audioConfig.startTime)
        {
            source.SetScheduledEndTime(AudioSettings.dspTime + audioConfig.endTime - audioConfig.startTime);
        }
    }

    private AudioSource GetSource()
    {
        AudioSource source = sfxPool[index];

        index++;

        if (index >= sfxPool.Length)
            index = 0;

        return source;
    }
}
