using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    public Light torchLight;

    [Header("Light")]
    public float baseIntensity = 2.2f;
    public float flickerAmount = 0.7f;
    public float flickerSpeed = 6f;

    [Header("Range")]
    public float baseRange = 5f;
    public float rangeFlicker = 0.4f;

    private Vector3 startPosition;

    void Start()
    {
        if (torchLight == null)
            torchLight = GetComponent<Light>();

        startPosition = transform.localPosition;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.3f);

        torchLight.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
        torchLight.range = baseRange + (noise - 0.5f) * rangeFlicker;

        transform.localPosition = startPosition + new Vector3(
            Mathf.Sin(Time.time * 13f) * 0.015f,
            Mathf.Sin(Time.time * 17f) * 0.02f,
            Mathf.Sin(Time.time * 11f) * 0.015f
        );
    }
}