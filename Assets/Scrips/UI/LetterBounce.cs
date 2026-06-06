using UnityEngine;

public class LetterBounce : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 0.12f;
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float rotationAmount = 2f;

    [SerializeField] private bool bounceDown = false;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private bool animating;
    private float timer;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!animating) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);
        float curve = Mathf.Sin(t * Mathf.PI);

        float direction = bounceDown ? -1f : 1f;

        transform.localPosition =
            startLocalPosition +
            Vector3.up * bounceHeight * curve * direction;

        transform.localRotation =
            startLocalRotation *
            Quaternion.Euler(
                0f,
                0f,
                rotationAmount * curve * direction);

        if (t >= 1f)
        {
            transform.localPosition = startLocalPosition;
            transform.localRotation = startLocalRotation;
            animating = false;
        }
    }

    public void PlayBounce()
    {
        timer = 0f;
        animating = true;
    }
}