using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DissolvingController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    [SerializeField] private float dissolveRate = 0.0125f;
    private float timeToStartDissolve = 5f;
    private bool isDissolving = false;
    float counter = 0f;


    private Material[] materials;

    private void Start()
    {
        if (targetRenderer != null)
        {
            materials = targetRenderer.materials;
        }
        timeToStartDissolve = Time.time + timeToStartDissolve;
    }

    private void Update()
    {
        
        if (Time.time>= timeToStartDissolve&& !isDissolving)
        { 
            isDissolving = true;
        }

        if (isDissolving&&counter<1f)
        {
            DissolveCo();
            
        }
    }

    private void DissolveCo()
    {
        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning("No materials found on the target renderer.");
        }
            counter += dissolveRate;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat("_DissolveAmount", counter);
            }
        
    }   
}