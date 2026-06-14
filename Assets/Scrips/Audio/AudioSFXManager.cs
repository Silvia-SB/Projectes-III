using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static AudioProfile;

public class AudioSFXManager : MonoBehaviour
{
    public static AudioSFXManager Instance { get; private set; }

    [Header("SFX Pool")]
    [SerializeField] private int poolSize = 30;

    [Header("3D Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 25f;
    
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    private AudioSource[] sfxPool;
    private int index;

    public AudioMixerGroup OutputMixerGroup => outputMixerGroup;

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
            source.loop = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.outputAudioMixerGroup = outputMixerGroup;

            sfxPool[i] = source;
        }
    }

    public static void PlayStatus(GameObject gameObject, DamageType type)
    {
        if (Instance == null || gameObject == null) return;

        AudioProfileReference profileRef = gameObject.GetComponent<AudioProfileReference>();

        if (profileRef == null)
            profileRef = gameObject.GetComponentInParent<AudioProfileReference>();

        if (profileRef == null) return;

        AudioConfig audioConfig = profileRef.GetClipFromProfile(type, gameObject);
        if(audioConfig.clip == null) return;

        Instance.PlaySFXAtPosition(audioConfig, gameObject.transform.position, audioConfig.volume);
    }

    public static void PlayExplosion(GameObject gameObject, DamageType type)
    {
        if (Instance == null || gameObject == null) return;

        AudioProfileReference profileRef = gameObject.GetComponent<AudioProfileReference>();

        if (profileRef == null)
            profileRef = gameObject.GetComponentInParent<AudioProfileReference>();

        if (profileRef == null) return;

        AudioConfig audioConfig = type == DamageType.Blood
            ? profileRef.Profile.FireExplosion
            : profileRef.GetClipFromProfile(type, gameObject);

        if (audioConfig.clip == null) return;

        Instance.PlaySFXAtPosition(audioConfig, gameObject.transform.position, audioConfig.volume);
    }

    public static void PlaySFX(Collider collider, Vector3 position, ArrowType arrowType, bool isFullyCharged)
    {
        if (Instance == null) return;

        AudioProfileReference profileRef = collider.GetComponent<AudioProfileReference>();
        
        if (profileRef == null)
            profileRef = collider.GetComponentInParent<AudioProfileReference>();

        if (profileRef == null) return;

        AudioConfig audioConfig = profileRef.GetClipFromProfile(arrowType, isFullyCharged);
        if(audioConfig.clip == null) return;

        Instance.PlaySFXAtPosition(audioConfig, position, audioConfig.volume);
    }
    
    private void PlaySFXAtPosition(AudioConfig audioConfig, Vector3 position, float volume)
    {
        if (audioConfig.clip == null) return;

        AudioSource source = GetSource();

        source.Stop();
        source.transform.position = position;
        source.volume = volume;
        source.pitch = 1f;
        source.loop = false;
        source.spatialBlend = 1f;
        source.clip = audioConfig.clip;
        source.time = Mathf.Clamp(audioConfig.startTime, 0f, Mathf.Max(0f, audioConfig.clip.length - 0.01f));
        source.maxDistance = audioConfig.maxDistance;

        source.Play();

        float duration = audioConfig.clip.length - source.time;

        if (audioConfig.endTime > audioConfig.startTime)
        {
            duration = audioConfig.endTime - audioConfig.startTime;
            source.SetScheduledEndTime(AudioSettings.dspTime + duration);
        }

        StartCoroutine(StopSourceAfter(source, audioConfig.clip, duration));
    }

    public AudioSource GetSource()
    {
        AudioSource source = sfxPool[index];

        index++;

        if (index >= sfxPool.Length)
            index = 0;

        return source;
    }

    private IEnumerator StopSourceAfter(AudioSource source, AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (source == null || source.clip != clip) yield break;

        source.Stop();
        source.clip = null;
    }
}
