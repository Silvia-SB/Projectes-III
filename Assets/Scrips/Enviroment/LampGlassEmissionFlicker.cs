using UnityEngine;

public class LampGlassEmissionFlicker : MonoBehaviour
{
    [Header("Renderer del cristal")]
    public Renderer glassRenderer;

    [Header("Emission")]
    public Color baseEmissionColor = new Color(1f, 0.55f, 0.18f);
    public float minEmission = 0.7f;
    public float maxEmission = 2.5f;
    public float flickerSpeed = 1.2f;

    [Header("Variation")]
    public bool randomizeOnStart = true;

    private MaterialPropertyBlock propertyBlock;
    private float randomOffset;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        if (glassRenderer == null)
            glassRenderer = GetComponent<Renderer>();

        propertyBlock = new MaterialPropertyBlock();
        randomOffset = Random.Range(0f, 100f);

        if (randomizeOnStart)
        {
            minEmission *= Random.Range(0.8f, 1.2f);
            maxEmission *= Random.Range(0.85f, 1.35f);
            flickerSpeed *= Random.Range(0.7f, 1.4f);
        }
    }

    void Update()
    {
        if (glassRenderer == null) return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
        float emissionIntensity = Mathf.Lerp(minEmission, maxEmission, noise);

        Color finalEmission = baseEmissionColor * emissionIntensity;

        glassRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(EmissionColorID, finalEmission);
        glassRenderer.SetPropertyBlock(propertyBlock);
    }
}