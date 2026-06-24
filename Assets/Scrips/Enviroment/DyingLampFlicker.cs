using UnityEngine;

public class DyingLampFlicker : MonoBehaviour
{
    [Header("Light")]
    public Light flickerLight;

    [Header("Weak Flame")]
    public float minIntensity = 0.15f;
    public float maxIntensity = 2.2f;
    public float flickerSpeed = 1.2f;

    [Header("Range")]
    public float minRange = 1.5f;
    public float maxRange = 5f;

    [Header("Color")]
    public Color weakColor = new Color(0.45f, 0.18f, 0.05f);
    public Color hotColor = new Color(1f, 0.45f, 0.12f);

    [Header("Long Dips")]
    public float dipPower = 2.8f;

    [Header("Occasional Flare")]
    public float flareChance = 0.008f;
    public float flareStrength = 1.5f;

    private float randomOffset;

    void Start()
    {
        randomOffset = Random.Range(0f, 100f);

        if (flickerLight == null)
            flickerLight = GetComponent<Light>();
    }

    void Update()
    {
        float slowNoise = Mathf.PerlinNoise(
            Time.time * flickerSpeed,
            randomOffset
        );

        float pulse = Mathf.Sin(
            Time.time * flickerSpeed * 0.7f + randomOffset
        ) * 0.5f + 0.5f;

        float mixed = Mathf.Lerp(slowNoise, pulse, 0.55f);

        // Hace que pase más tiempo casi apagada
        float dyingFlame = Mathf.Pow(mixed, dipPower);

        float flare = 0f;

        if (Random.value < flareChance)
            flare = Random.Range(0.5f, flareStrength);

        flickerLight.intensity =
            Mathf.Lerp(minIntensity, maxIntensity, dyingFlame) + flare;

        flickerLight.range =
            Mathf.Lerp(minRange, maxRange, dyingFlame);

        flickerLight.color =
            Color.Lerp(weakColor, hotColor, dyingFlame);
    }
}