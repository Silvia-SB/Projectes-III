using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider))]
public class RendererFeatureTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string targetTag = "Player";
    
    [SerializeField] private bool activateFeature = false;

    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";

    [Header("Fog Fade")]
    [SerializeField] private Material fogMaterial;
    [SerializeField] private string fogStrengthProperty = "_FogStrength";
    [SerializeField, Min(0f)] private float fadeDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float inactiveStrength = 0f;
    [SerializeField, Range(0f, 1f)] private float activeStrength = 1f;

    private int fogStrengthID;
    private bool isFading;
    private bool deactivateFeatureAfterFade;
    private float fadeTimer;
    private float fadeStartStrength;
    private float fadeTargetStrength;

    private void Awake()
    {
        fogStrengthID = Shader.PropertyToID(fogStrengthProperty);
        ResolveFogMaterial();

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!isFading) return;

        fadeTimer += Time.deltaTime;
        float normalizedTime = fadeDuration <= 0f ? 1f : Mathf.Clamp01(fadeTimer / fadeDuration);
        float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);

        SetFogStrength(Mathf.Lerp(fadeStartStrength, fadeTargetStrength, smoothTime));

        if (normalizedTime < 1f) return;

        isFading = false;

        if (deactivateFeatureAfterFade)
        {
            URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            if (activateFeature)
            {
                activateFeature = false;
                StartFogFade(inactiveStrength, true);
            }
            else
            {
                activateFeature = true;
                URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, true);
                StartFogFade(activeStrength, false);
            }
        }
    }

    private void StartFogFade(float targetStrength, bool turnFeatureOffWhenDone)
    {
        ResolveFogMaterial();

        if (!HasFogStrengthProperty())
        {
            URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, !turnFeatureOffWhenDone);
            return;
        }

        fadeTimer = 0f;
        fadeStartStrength = fogMaterial.GetFloat(fogStrengthID);
        fadeTargetStrength = targetStrength;
        deactivateFeatureAfterFade = turnFeatureOffWhenDone;

        if (fadeDuration <= 0f || Mathf.Approximately(fadeStartStrength, fadeTargetStrength))
        {
            SetFogStrength(fadeTargetStrength);

            if (deactivateFeatureAfterFade)
            {
                URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, false);
            }

            isFading = false;
            return;
        }

        isFading = true;
    }

    private void SetFogStrength(float value)
    {
        if (!HasFogStrengthProperty()) return;
        fogMaterial.SetFloat(fogStrengthID, Mathf.Clamp01(value));
    }

    private bool HasFogStrengthProperty()
    {
        return fogMaterial != null && fogMaterial.HasProperty(fogStrengthID);
    }

    private void ResolveFogMaterial()
    {
        if (fogMaterial != null || pcRendererData == null) return;

        foreach (ScriptableRendererFeature feature in pcRendererData.rendererFeatures)
        {
            if (feature == null || feature.name != fullScreenFeatureName) continue;

            FieldInfo materialField = feature.GetType().GetField(
                "passMaterial",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (materialField == null) return;

            fogMaterial = materialField.GetValue(feature) as Material;
            return;
        }
    }
}
