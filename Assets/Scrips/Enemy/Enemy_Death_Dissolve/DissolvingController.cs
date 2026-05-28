using System.Collections;
using UnityEngine;

public class DissolvingController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private float dissolveRate = 0.0125f;
    [SerializeField] private float refreshRate = 0.025f;

    private Material[] materials;

    private void Start()
    {
        if (meshRenderer != null)
        {
            materials = meshRenderer.materials;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DissolveCo());
        }
    }

    private IEnumerator DissolveCo()
    {
        if (materials == null || materials.Length == 0)
            yield break;

        float counter = 0;

        while (materials[0].GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat("_DissolveAmount", counter);
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
