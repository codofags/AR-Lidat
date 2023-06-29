using UnityEngine;

public static class MeshFilterExtensions
{
    public static void GenerateUV(this MeshFilter meshFilter, Camera camera)
    {
        Debug.Log("Generate UV");
        Mesh mesh = new Mesh();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] normals = meshFilter.mesh.normals;
        int[] faces = meshFilter.mesh.triangles;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = faces;


        Vector2[] textureCoordinates = CalcTextureCoordinates(mesh, meshFilter.transform.localToWorldMatrix, camera);
        mesh.uv = textureCoordinates;
        meshFilter.mesh = mesh;
    }

    public static void GenerateUV(this MeshFilter meshFilter, Camera camera, Texture2D texture)
    {
        Debug.Log("Generate UV");
        Mesh mesh = new Mesh();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] normals = meshFilter.mesh.normals;
        int[] faces = meshFilter.mesh.triangles;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = faces;


        Vector2[] textureCoordinates = CalcTextureCoordinates(mesh, meshFilter.transform.localToWorldMatrix, camera, texture);
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

    //public static void TextureMesh(this MeshFilter meshFilter, Texture texture)
    //{
    //    meshFilter.GenerateUV();
    //    meshFilter.TexturedMesh(texture);
    //}

    private static Vector2[] CalcTextureCoordinates(Mesh geometry, Matrix4x4 modelMatrix, Camera camera)
    {

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

    private static Vector2[] CalcTextureCoordinates(Mesh geometry, Matrix4x4 modelMatrix, Camera camera, Texture2D texture)
    {

        Vector2[] textureCoordinates = new Vector2[geometry.vertices.Length];
        Vector2 screenSize = new Vector2(texture.width, texture.height);

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

    public static bool IsMeshFullyInCamera(this MeshFilter meshFilter, Camera camera, Vector3 camPosition, Quaternion camRotation)
    {
        camera.transform.localPosition = camPosition;
        camera.transform.localRotation = camRotation;
        Bounds bounds = meshFilter.mesh.bounds;
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

        // Получаем все вершины меша в мировых координатах
        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = meshFilter.transform.TransformPoint(vertices[i]);
        }

        // Проверяем, что все вершины меша находятся внутри видимого объема камеры
        for (int i = 0; i < worldVertices.Length; i++)
        {
            if (!GeometryUtility.TestPlanesAABB(cameraPlanes, new Bounds(worldVertices[i], Vector3.zero)))
            {
                return false;
            }
        }

        // Проверяем, что ограничивающий объем меша полностью находится в видимом объеме камеры
        if (!GeometryUtility.TestPlanesAABB(cameraPlanes, bounds))
        {
            return false;
        }

        return true;
    }
}