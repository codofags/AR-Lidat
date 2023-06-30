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

    public static bool IsCameraLookingAtObject(this Camera camera, GameObject target)
    {
        Vector3 cameraToObject = target.transform.position - camera.transform.position;
        cameraToObject.Normalize();

        Vector3 cameraForward = camera.transform.forward;
        cameraForward.Normalize();

        // Вычисляем скалярное произведение векторов
        float dotProduct = Vector3.Dot(cameraToObject, cameraForward);

        // Проверяем угол между векторами
        // Если угол близок к 0 градусам, камера смотрит прямо на объект
        float angleThreshold = 0.99f; // Пороговое значение угла (примерно 0.99 соответствует 8 градусам)
        return dotProduct >= angleThreshold;
    }

    public static float GetCameraAngleLookingAtObject(this Camera camera, GameObject target)
    {
        Vector3 cameraToObject = target.transform.position - camera.transform.position;
        cameraToObject.Normalize();

        Vector3 cameraForward = camera.transform.forward;
        cameraForward.Normalize();

        // Вычисляем скалярное произведение векторов
        float dotProduct = Vector3.Dot(cameraToObject, cameraForward);

        return dotProduct;
    }
}