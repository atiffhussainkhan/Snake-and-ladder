using UnityEngine;

namespace SnakeLadder.Prototype
{
    // Shared low-poly mesh builders. Keep allocations tight — every tube allocates one
    // Vector3[] / Vector2[] for vertices/uvs and one int[] for triangles.
    public static class MeshUtils
    {
        // Builds a smooth tube along `points` with per-sample radius. Used for snake bodies
        // and ladder rails. Caller owns the returned GameObject's lifetime.
        public static GameObject CreateTube(Transform parent, string name, Vector3[] points, float[] radii, Material material, int radialSegments = 12)
        {
            int vertexCount = points.Length * radialSegments;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals  = new Vector3[vertexCount];
            Vector2[] uvs      = new Vector2[vertexCount];

            Vector3[] tangents = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                    tangents[i] = (points[i + 1] - points[i]).normalized;
                else if (i == points.Length - 1)
                    tangents[i] = (points[i] - points[i - 1]).normalized;
                else
                    tangents[i] = (points[i + 1] - points[i - 1]).normalized;
            }

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 forward = tangents[i];
                Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up;
                Vector3 right = Vector3.Cross(forward, up).normalized;
                up = Vector3.Cross(right, forward).normalized;

                for (int j = 0; j < radialSegments; j++)
                {
                    float angle = (j / (float)radialSegments) * 2f * Mathf.PI;
                    Vector3 offset = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    int idx = i * radialSegments + j;
                    vertices[idx] = points[i] + offset * radii[i];
                    normals[idx] = offset;
                    uvs[idx] = new Vector2(j / (float)radialSegments, i / (float)(points.Length - 1));
                }
            }

            int[] triangles = new int[(points.Length - 1) * radialSegments * 6];
            int t = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int j1 = (j + 1) % radialSegments;
                    int a = i * radialSegments + j;
                    int b = i * radialSegments + j1;
                    int c = (i + 1) * radialSegments + j;
                    int d = (i + 1) * radialSegments + j1;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }

            var mesh = new Mesh { name = name };
            mesh.MarkDynamic();
            mesh.vertices = vertices;
            mesh.normals  = normals;
            mesh.uv       = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
