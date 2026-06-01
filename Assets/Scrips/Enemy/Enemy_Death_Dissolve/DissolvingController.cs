using System;
using System.Collections.Generic;
using UnityEngine;

public class DissolvingController : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;

    [SerializeField] private float dissolveSpeed = 0.5f;
    private bool isDissolving = false;
    private float counter = 0f;
    
    public Action OnDissolveComplete;

    private List<Material> materials = new List<Material>();
    
    // OPTIMIZACIÓN: Precalculamos el ID de la propiedad del shader para no usar strings en el Update
    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            // Añadimos 'true' para que encuentre las mallas incluso si el enemigo nace desactivado por la Pool
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        foreach (Renderer rend in targetRenderers)
        {
            if (rend != null && !(rend is ParticleSystemRenderer))
            {
                materials.AddRange(rend.materials);
            }
        }
    }

    private void Update()
    {
        if (isDissolving && counter < 1f)
        {
            DissolveCo();
        }
    }

    private void DissolveCo()
    {
        // Si por alguna razón no hay materiales, devolvemos el enemigo a la pool directamente para que no se quede atascado
        if (materials.Count == 0) 
        {
            isDissolving = false;
            OnDissolveComplete?.Invoke();
            return;
        }
        
        counter += dissolveSpeed * Time.deltaTime;
        foreach (Material mat in materials)
        {
            // Utilizamos el ID cacheado en lugar del string
            if (mat != null) mat.SetFloat(DissolveAmountID, counter);
        }
        
        if (counter >= 1f)
        {
            isDissolving = false;
            OnDissolveComplete?.Invoke();
        }
    }

    public void StartDissolve()
    {
        isDissolving = true;
    }

    public void ResetDissolve()
    {
        isDissolving = false;
        counter = 0f;
        foreach (Material mat in materials)
        {
            if (mat != null) mat.SetFloat(DissolveAmountID, counter);
        }
    }   
}