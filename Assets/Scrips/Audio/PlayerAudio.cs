using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource bowStringSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Movement Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private float movementMinVelocity = 0.15f;
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float runVolume = 0.9f;

    [Header("Bow Clips")]
    [SerializeField] private AudioClip bowStringTensionClip;
    [SerializeField] private AudioClip shootClip;
    [SerializeField, Range(0f, 1f)] private float bowStringVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float shootVolume = 1f;

    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerShooter == null) playerShooter = GetComponent<PlayerShooter>();

        movementSource = GetOrCreateAudioSource(movementSource);
        bowStringSource = GetOrCreateAudioSource(bowStringSource);
        oneShotSource = GetOrCreateAudioSource(oneShotSource);

        movementSource.loop = true;
        bowStringSource.loop = true;
        oneShotSource.loop = false;
    }

    private void OnEnable()
    {
        if (playerShooter == null) return;

        playerShooter.OnChargeStart += PlayBowStringTension;
        playerShooter.OnChargeEnd += StopBowStringTension;
        playerShooter.OnChargeCanceled += StopBowStringTension;
        playerShooter.OnShootTriggered += PlayShoot;
    }

    private void OnDisable()
    {
        if (playerShooter != null)
        {
            playerShooter.OnChargeStart -= PlayBowStringTension;
            playerShooter.OnChargeEnd -= StopBowStringTension;
            playerShooter.OnChargeCanceled -= StopBowStringTension;
            playerShooter.OnShootTriggered -= PlayShoot;
        }

        StopMovement();
        StopBowStringTension();
    }

    private void Update()
    {
        UpdateMovementAudio();
    }

    private void UpdateMovementAudio()
    {
        if (playerMovement == null || playerMovement.Controller == null || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        Vector3 horizontalVelocity = playerMovement.Controller.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.magnitude > movementMinVelocity && playerMovement.IsGrounded;
        if (!isMoving)
        {
            StopMovement();
            return;
        }

        bool isRunning = playerMovement.IsSprinting && !playerMovement.IsSlowed && !playerMovement.IsChargingArrow;
        AudioClip targetClip = isRunning ? runClip : walkClip;

        if (targetClip == null)
        {
            StopMovement();
            return;
        }

        movementSource.volume = isRunning ? runVolume : walkVolume;

        if (movementSource.clip != targetClip)
        {
            movementSource.clip = targetClip;
            movementSource.Play();
        }
        else if (!movementSource.isPlaying)
        {
            movementSource.Play();
        }
    }

    private void PlayBowStringTension()
    {
        if (bowStringTensionClip == null || bowStringSource == null) return;

        bowStringSource.clip = bowStringTensionClip;
        bowStringSource.volume = bowStringVolume;
        bowStringSource.Play();
    }

    private void StopBowStringTension()
    {
        if (bowStringSource == null) return;

        bowStringSource.Stop();
    }

    private void PlayShoot()
    {
        if (shootClip == null || oneShotSource == null) return;

        oneShotSource.PlayOneShot(shootClip, shootVolume);
    }

    private void StopMovement()
    {
        if (movementSource == null || !movementSource.isPlaying) return;

        movementSource.Stop();
    }

    private AudioSource GetOrCreateAudioSource(AudioSource source)
    {
        if (source != null) return source;

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (AudioSFXManager.Instance != null)
            audioSource.outputAudioMixerGroup = AudioSFXManager.Instance.OutputMixerGroup;

        return audioSource;
    }
}