using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class URPUtility
{
    public static void SetRendererFeatureActive(ScriptableRendererData rendererData, string featureName, bool active)
    {
        if (rendererData == null)
        {
            Debug.LogWarning("Renderer Data not assigned.");
            return;
        }

        foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
        {
            if (feature == null) continue;

            if (feature.name == featureName)
            {
                feature.SetActive(active);
                rendererData.SetDirty();
                return;
            }
        }

        Debug.LogWarning("Renderer Feature not found: " + featureName);
    }
}
