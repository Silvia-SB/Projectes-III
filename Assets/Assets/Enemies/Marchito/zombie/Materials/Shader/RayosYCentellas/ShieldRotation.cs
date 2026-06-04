using UnityEngine;

public class RotateMaterial : MonoBehaviour
{
    public Material material; // Assign the material in the Inspector
    public float rotationSpeed = 10f; // Rotation speed in degrees per second
    private float angle = 0f; // Store the current angle of rotation

    void Start()
    {
        if (material == null)
            material = GetComponent<Renderer>().material; // Get the material from the object if not assigned in inspector
    }

    void Update()
    {
        // Rotate the texture on the Y-axis
        angle += rotationSpeed * Time.deltaTime;

        // Apply the rotation to the material's texture using mainTextureOffset
        float radians = angle * Mathf.Deg2Rad; // Convert degrees to radians
        Vector2 rotationOffset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        material.SetTextureOffset("_MainTex", rotationOffset); // Rotate main texture
    }
}