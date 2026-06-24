using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InicializeFog : MonoBehaviour
{
    [Header("URP Full Screen Pass")]
    [SerializeField] private ScriptableRendererData pcRendererData;
    [SerializeField] private string fullScreenFeatureName = "FullScreenPassRendererFeature";
    
    [Header("Fog Fade")]
    [SerializeField] private Material fogMaterial;
    [SerializeField] private string fogStrengthProperty = "_FogStrength";
    private float enterFogStrength = 1f;
    private int fogStrengthID;

    private void Awake()
    {
        fogStrengthID = Shader.PropertyToID(fogStrengthProperty);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fogMaterial != null && fogMaterial.HasProperty(fogStrengthID))
            {
                if (Mathf.Approximately(fogMaterial.GetFloat(fogStrengthID), enterFogStrength))
                {
                    return;
                }

                URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, true);
                fogMaterial.SetFloat(fogStrengthID, enterFogStrength);
                return;
            }

            URPUtility.SetRendererFeatureActive(pcRendererData, fullScreenFeatureName, true);
        }
    }
}
