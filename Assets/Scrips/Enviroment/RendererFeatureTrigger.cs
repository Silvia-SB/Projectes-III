using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider))]
public class RendererFeatureTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string targetTag = "Player";
    
    [SerializeField] private bool activateFeature = true;

    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, activateFeature);
            hasTriggered = true;
        }
    }
}