using UnityEngine;

public class InteractableRaycastDetector : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 4f;
    [SerializeField] private LayerMask interactableLayer;

    private InteractableHighlighter currentHighlight;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        DetectInteractable();
    }

    private void DetectInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            InteractableHighlighter newHighlight = hit.collider.GetComponentInParent<InteractableHighlighter>();

            if (newHighlight != currentHighlight)
            {
                ClearCurrentHighlight();

                currentHighlight = newHighlight;

                if (currentHighlight != null)
                    currentHighlight.SetHighlight(true);
            }
        }
        else
        {
            ClearCurrentHighlight();
        }
    }

    private void ClearCurrentHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
            currentHighlight = null;
        }
    }
}