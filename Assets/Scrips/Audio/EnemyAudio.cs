using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [System.Serializable]
    public struct AudioConfig
    {
        public AudioClip clip;
        [Min(0f)]
        public float startTime;
        [Min(0f)]
        public float endTime;
        [Range(0f, 1f)]
        public float volume;
        public bool loop;
    }

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioConfig walkSound;
    [SerializeField] private AudioConfig attackSound;
    [SerializeField] private AudioConfig deathSound;

    private float currentEndTime;
    private bool shouldStopAtEndTime;

    private void Awake()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!shouldStopAtEndTime || source == null || !source.isPlaying) return;

        if (source.time >= currentEndTime)
        {
            source.Stop();
            shouldStopAtEndTime = false;
        }
    }

    public void PlayWalkSound()
    {
        PlaySound(walkSound);
    }

    public void PlayAttackSound()
    {
        if(attackSound.clip == null) PlayWalkSound();
        PlaySound(attackSound);
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    private void PlaySound(AudioConfig config)
    {
        if (source == null || config.clip == null) return;

        float startTime = Mathf.Clamp(config.startTime, 0f, config.clip.length);
        float endTime = config.endTime <= 0f
            ? config.clip.length
            : Mathf.Clamp(config.endTime, startTime, config.clip.length);

        source.clip = config.clip;
        source.time = startTime;
        source.volume = config.volume;
        source.loop = config.loop;
        source.Play();

        currentEndTime = endTime;
        shouldStopAtEndTime = !config.loop;
    }
}
