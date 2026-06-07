using UnityEngine;

public class InteractableHighlighter : MonoBehaviour
{
    [Header("Material de contorno")]
    [SerializeField] private Material highlightMaterial;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private bool isHighlighted;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
        }
    }

    public void SetHighlight(bool active)
    {
        if (isHighlighted == active) return;

        isHighlighted = active;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (active)
            {
                Material[] currentMaterials = originalMaterials[i];
                Material[] newMaterials = new Material[currentMaterials.Length + 1];

                for (int j = 0; j < currentMaterials.Length; j++)
                {
                    newMaterials[j] = currentMaterials[j];
                }

                newMaterials[newMaterials.Length - 1] = highlightMaterial;
                renderers[i].materials = newMaterials;
            }
            else
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void OnDisable()
    {
        SetHighlight(false);
    }
}