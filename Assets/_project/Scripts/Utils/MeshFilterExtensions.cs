using System.Collections.Generic;
using UnityEngine;

public static class MeshFilterExtensions
{
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
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }

    public static void GenerateUV_2(this MeshFilter meshFilter, Camera camera, Texture2D texture)
    {
        Mesh mesh = meshFilter.mesh;
        Vector2[] uv = new Vector2[mesh.vertices.Length];

        Matrix4x4 worldToScreenMatrix = camera.worldToCameraMatrix * camera.projectionMatrix;

        for (int i = 0; i < uv.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            Vector3 screenPos = worldToScreenMatrix.MultiplyPoint(meshFilter.transform.TransformPoint(vertex));
            Vector2 normalizedScreenPos = new Vector2(screenPos.x / texture.width, screenPos.y / texture.height);
            uv[i] = normalizedScreenPos;
        }

        mesh.uv = uv;
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

            // Проверка на наличие текстуры и ее размер
            if (texture != null && texture.width > 0 && texture.height > 0)
            {
                // Проверка на нахождение точки в пределах экрана
                if (screenPoint.x >= 0 && screenPoint.x <= screenSize.x && screenPoint.y >= 0 && screenPoint.y <= screenSize.y)
                {
                    float u = screenPoint.x / screenSize.x;
                    float v = 1 - screenPoint.y / screenSize.y;

                    // Проверка на наличие пикселя в текстуре
                    if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
                    {
                        // Преобразование координаты (0,0) в левый верхний угол текстуры
                        Vector2 textureCoordinate = new Vector2(u, v);
                        textureCoordinates[i] = textureCoordinate;
                    }
                    else
                    {
                        textureCoordinates[i] = Vector2.zero;
                    }
                }
                else
                {
                    textureCoordinates[i] = Vector2.zero;
                }
            }
            else
            {
                textureCoordinates[i] = Vector2.zero;
            }
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
    public static Mesh Extract(this Mesh m, int meshIndex)
    {
        var vertices = m.vertices;
        var normals = m.normals;

        var newVerts = new List<Vector3>();
        var newNorms = new List<Vector3>();
        var newTris = new List<int>();
        var triangles = m.GetTriangles(meshIndex);
        for (var i = 0; i < triangles.Length; i += 3)
        {
            var A = triangles[i + 0];
            var B = triangles[i + 1];
            var C = triangles[i + 2];
            newVerts.Add(vertices[A]);
            newVerts.Add(vertices[B]);
            newVerts.Add(vertices[C]);
            newNorms.Add(normals[A]);
            newNorms.Add(normals[B]);
            newNorms.Add(normals[C]);
            newTris.Add(newTris.Count);
            newTris.Add(newTris.Count);
            newTris.Add(newTris.Count);
        }
        var mesh = new Mesh();
        mesh.indexFormat = newVerts.Count > 65536 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(newVerts);
        mesh.SetNormals(newNorms);
        mesh.SetTriangles(newTris, 0, true);
        return mesh;
    }
}
