using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateStarMesh : MonoBehaviour
{
    [Header("Stars")]
    public int starCount = 120;
    public float radius = 450f;
    public float minHeight = 80f;
    public Vector2 sizeRange = new Vector2(0.8f, 2.2f);

    [Header("Color")]
    public Color starColor = new Color(0.78f, 0.86f, 1f, 1f);

    [ContextMenu("Generate Stars Mesh")]
    public void Generate()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        Camera cam = Camera.main;
        Vector3 cameraForward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 cameraRight = cam != null ? cam.transform.right : Vector3.right;
        Vector3 cameraUp = cam != null ? cam.transform.up : Vector3.up;

        for (int i = 0; i < starCount; i++)
        {
            Vector3 direction = Random.onUnitSphere;

            if (direction.y < 0.2f)
                direction.y = Random.Range(0.2f, 1f);

            direction.Normalize();

            Vector3 center = direction * radius;

            if (center.y < minHeight)
                center.y = minHeight + Random.Range(0f, 120f);

            float size = Random.Range(sizeRange.x, sizeRange.y);

            int index = vertices.Count;

            vertices.Add(center - cameraRight * size - cameraUp * size);
            vertices.Add(center + cameraRight * size - cameraUp * size);
            vertices.Add(center + cameraRight * size + cameraUp * size);
            vertices.Add(center - cameraRight * size + cameraUp * size);

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);

            float brightness = Random.Range(0.6f, 1.3f);
            Color finalColor = starColor * brightness;
            finalColor.a = 1f;

            colors.Add(finalColor);
            colors.Add(finalColor);
            colors.Add(finalColor);
            colors.Add(finalColor);
        }

        Mesh mesh = new Mesh();
        mesh.name = "Generated_StarMesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}