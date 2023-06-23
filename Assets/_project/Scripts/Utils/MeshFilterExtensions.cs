using UnityEngine;

public static class MeshFilterExtensions
{
    public static void GenerateUV(this MeshFilter meshFilter)
    {
        Debug.Log("Generate UV");
        Mesh mesh = new Mesh();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] normals = meshFilter.mesh.normals;
        int[] faces = meshFilter.mesh.triangles;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = faces;


        Vector2[] textureCoordinates = CalcTextureCoordinates(mesh, meshFilter.transform.localToWorldMatrix);
        mesh.uv = textureCoordinates;
        meshFilter.mesh = mesh;
    }

    public static void TexturedMesh(this MeshFilter meshFilter, Texture texture)
    {
        Debug.Log("Start Textured.");
        var renderer = meshFilter.GetComponent<MeshRenderer>();
        var material = new Material(renderer.material);

        material.mainTexture = texture;
        renderer.material = material;
    }

    public static void TextureMesh(this MeshFilter meshFilter, Texture texture)
    {
        meshFilter.GenerateUV();
        meshFilter.TexturedMesh(texture);
    }

    private static Vector2[] CalcTextureCoordinates(Mesh geometry, Matrix4x4 modelMatrix)
    {
        var camera = Camera.main;

        Vector2[] textureCoordinates = new Vector2[geometry.vertices.Length];
        Vector2 screenSize = new Vector2(camera.pixelWidth, camera.pixelHeight);

        for (int i = 0; i < geometry.vertices.Length; i++)
        {
            Vector3 vertex = geometry.vertices[i];
            Vector4 vertex4 = new Vector4(vertex.x, vertex.y, vertex.z, 1f);
            Vector4 worldVertex4 = modelMatrix * vertex4;
            Vector3 worldVector3 = new Vector3(worldVertex4.x, worldVertex4.y, worldVertex4.z);
            Vector3 screenPoint = camera.WorldToScreenPoint(worldVector3);
            float u = screenPoint.x / screenSize.x;
            float v = (screenPoint.y / screenSize.y);
            textureCoordinates[i] = new Vector2(u, v);
        }

        return textureCoordinates;
    }
}