using UnityEngine;

public class BannerSimpleSway : MonoBehaviour
{
    [Header("Principal Movement")]
    public float swayAmount = 2f;
    public float swaySpeed = 0.8f;

    [Header("Secondary Movement")]
    public float secondaryAmount = 0.5f;
    public float secondarySpeed = 1.4f;

    private Quaternion startRotation;
    private float randomOffset;

    void Start()
    {
        startRotation = transform.localRotation;
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float swayZ = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAmount;
        float swayX = Mathf.Sin(Time.time * secondarySpeed + randomOffset) * secondaryAmount;

        transform.localRotation = startRotation * Quaternion.Euler(
            swayX,
            0f,
            swayZ
        );
    }
}