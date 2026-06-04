using UnityEngine;

public class NeonEdgeGlowController : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color neonColor = Color.cyan;
    public float intensity = 2.0f;
    public float fresnelPower = 1.5f;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        UpdateNeonEffect();
    }

    void Update()
    {
        // Example: Change color with keyboard input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            neonColor = new Color(Random.value, Random.value, Random.value);
            UpdateNeonEffect();
        }
    }

    void UpdateNeonEffect()
    {
        if (targetRenderer != null && targetRenderer.material != null)
        {
            targetRenderer.material.SetColor("_Color", neonColor);
            targetRenderer.material.SetFloat("_Intensity", intensity);
            targetRenderer.material.SetFloat("_FresnelPower", fresnelPower);
        }
    }
}
