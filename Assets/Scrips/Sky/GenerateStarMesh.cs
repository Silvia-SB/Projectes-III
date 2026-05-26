using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateStarMesh : MonoBehaviour
{
    public int starCount = 160;
    public float radius = 450f;
    public Vector2 sizeRange = new Vector2(1.2f, 3.2f);
    public int starSegments = 8;
    public Color starColor = new Color(0.85f, 0.92f, 1f, 1f);

    [ContextMenu("Generate Stars Mesh")]
    public void Generate()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        for (int i = 0; i < starCount; i++)
        {
            Vector3 direction;

            do
            {
                direction = Random.onUnitSphere;
            }
            while (direction.y < 0.15f);

            Vector3 center = direction.normalized * radius;

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.right;

            Vector3 up = Vector3.Cross(direction, right).normalized;

            float size = Random.Range(sizeRange.x, sizeRange.y);
            float brightness = Random.Range(1.5f, 4f);

            Color c = starColor * brightness;
            c.a = 1f;

            int centerIndex = vertices.Count;

            vertices.Add(center);
            colors.Add(c);

            for (int s = 0; s < starSegments; s++)
            {
                float angle = (Mathf.PI * 2f / starSegments) * s;
                Vector3 p = center 
                            + right * Mathf.Cos(angle) * size
                            + up * Mathf.Sin(angle) * size;

                vertices.Add(p);
                colors.Add(c);
            }

            for (int s = 0; s < starSegments; s++)
            {
                int current = centerIndex + 1 + s;
                int next = centerIndex + 1 + ((s + 1) % starSegments);

                triangles.Add(centerIndex);
                triangles.Add(next);
                triangles.Add(current);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Generated_Round_StarSky";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}