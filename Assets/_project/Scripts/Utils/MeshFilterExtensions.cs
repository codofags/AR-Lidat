using System.Collections.Generic;
using UnityEngine;

public static class MeshFilterExtensions
{
    public static void GenerateUV(this MeshFilter meshFilter, Camera camera, Texture2D texture, Vector2 offset)
    {
        Debug.Log("Generate UV");
        Mesh mesh = new Mesh();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] normals = meshFilter.mesh.normals;
        int[] faces = meshFilter.mesh.triangles;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = faces;

        Vector2[] textureCoordinates = CalcTextureCoordinates(mesh, meshFilter.transform.localToWorldMatrix, camera, texture, offset);
        mesh.uv = textureCoordinates;
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }

    private static Vector2[] CalcTextureCoordinates(Mesh geometry, Matrix4x4 modelMatrix, Camera camera, Texture2D texture, Vector2 offset)
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

            // Проверка на наличие текстуры и ее размер
            if (texture != null && texture.width > 0 && texture.height > 0)
            {
                // Изменение вычисления координаты v для правильного отображения текстуры
                float u = screenPoint.x / screenSize.x;
                float v = screenPoint.y / screenSize.y;

                // Проверка ориентации камеры для коррекции ориентации текстуры
                if (Vector3.Dot(camera.transform.up, Vector3.up) < 0)
                {
                    v = 1 - v;
                }

                // Проверка на наличие пикселя в текстуре
                if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
                {
                    // Преобразование координаты (0,0) в левый верхний угол текстуры
                    Vector2 textureCoordinate = new Vector2(u + offset.x, v + offset.y);
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

        return textureCoordinates;
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
