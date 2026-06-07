using UnityEngine;

public class InteractableRaycastDetector : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 7f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Smoothing")]
    [SerializeField] private float deactivationDelay = 0.30f;

    private InteractableHighlighter currentHighlight;
    private float delayTimer;

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

            if (newHighlight != null)
            {
                delayTimer = 0f;

                if (newHighlight != currentHighlight)
                {
                    ClearCurrentHighlight();
                    currentHighlight = newHighlight;
                    currentHighlight.SetHighlight(true);
                }
            }
            else
            {
                SmoothDeactivate();
            }
        }
        else
        {
            SmoothDeactivate();
        }
    }

    private void SmoothDeactivate()
    {
        if (currentHighlight != null)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= deactivationDelay)
            {
                ClearCurrentHighlight();
                delayTimer = 0f;
            }
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