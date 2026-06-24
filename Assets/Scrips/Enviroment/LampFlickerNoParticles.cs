using UnityEngine;

public class LampFlickerNoParticles : MonoBehaviour
{
    [Header("Light")]
    public Light flickerLight;

    [Header("Intensity")]
    public float baseIntensity = 3.5f;
    public float flickerAmount = 2.5f;
    public float flickerSpeed = 12f;

    [Header("Range")]
    public float baseRange = 6f;
    public float rangeFlicker = 2.5f;

    [Header("Color")]
    public Color warmColor = new Color(1f, 0.72f, 0.32f);
    public Color hotColor = new Color(1f, 0.38f, 0.08f);

    [Header("Lamp Sway")]
    public Transform lampVisual;
    public float swayAmount = 5f;
    public float swaySpeed = 1.8f;

    [Header("Random Flicker Spikes")]
    public bool useSpikes = true;
    public float spikeChance = 0.025f;
    public float spikePower = 2f;

    private float randomOffset;
    private Quaternion startRotation;

    void Start()
    {
        randomOffset = Random.Range(0f, 100f);

        if (flickerLight == null)
            flickerLight = GetComponentInChildren<Light>();

        if (lampVisual != null)
            startRotation = lampVisual.localRotation;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(
            Time.time * flickerSpeed,
            randomOffset
        );

        float pulse = Mathf.Sin(
            Time.time * flickerSpeed * 1.9f + randomOffset
        ) * 0.5f + 0.5f;

        float finalNoise = Mathf.Lerp(noise, pulse, 0.55f);

        float spike = 0f;

        if (useSpikes && Random.value < spikeChance)
        {
            spike = Random.Range(0.5f, spikePower);
        }

        if (flickerLight != null)
        {
            flickerLight.intensity =
                baseIntensity +
                (finalNoise - 0.5f) * flickerAmount +
                spike;

            flickerLight.range =
                baseRange +
                (finalNoise - 0.5f) * rangeFlicker;

            flickerLight.color =
                Color.Lerp(warmColor, hotColor, finalNoise);
        }

        if (lampVisual != null)
        {
            float swayX = Mathf.Sin((Time.time + randomOffset) * swaySpeed) * swayAmount;
            float swayZ = Mathf.Sin((Time.time + randomOffset) * swaySpeed * 0.7f) * swayAmount * 0.45f;

            lampVisual.localRotation =
                startRotation *
                Quaternion.Euler(
                    swayX,
                    0f,
                    swayZ
                );
        }
    }
}