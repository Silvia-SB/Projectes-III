using UnityEngine;

public class BannerSway : MonoBehaviour
{
    public float swayAmount = 2f;
    public float swaySpeed = 0.8f;
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
        float mainSway = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAmount;
        float secondarySway = Mathf.Sin(Time.time * secondarySpeed + randomOffset) * secondaryAmount;

        transform.localRotation = startRotation * Quaternion.Euler(
            mainSway,
            0f,
            secondarySway
        );
    }
}