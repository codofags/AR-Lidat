using UnityEngine;

public static class CameraExtension
{
    public static bool IsMeshFullyIn(this Camera camera, MeshFilter meshFilter)
    {
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