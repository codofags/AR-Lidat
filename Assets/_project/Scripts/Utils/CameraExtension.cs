using UnityEngine;

public static class CameraExtension
{
    public static bool IsMeshFullyIn(this Camera camera, MeshFilter meshFilter)
    {
        // Получаем границы меша
        Bounds bounds = meshFilter.mesh.bounds;

        // Проверяем, что все вершины меша находятся внутри видимого объема камеры
        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            // Получаем вершину меша в мировых координатах
            Vector3 worldVertex = meshFilter.transform.TransformPoint(vertices[i]);

            // Проверяем, что вершина находится внутри видимого объема камеры
            if (!camera.IsPointVisible(worldVertex))
            {
                return false;
            }
        }

        // Проверяем, что ограничивающий объем меша полностью находится в видимом объеме камеры
        if (!camera.IsBoundsVisible(bounds))
        {
            return false;
        }

        return true;
    }

    // Метод для проверки видимости точки в видимом объеме камеры
    private static bool IsPointVisible(this Camera camera, Vector3 point)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(point);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }

    // Метод для проверки видимости границы в видимом объеме камеры
    private static bool IsBoundsVisible(this Camera camera, Bounds bounds)
    {
        // Получаем плоскости обзора камеры
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

        // Проверяем, что все 6 граней границы находятся внутри видимого объема камеры
        return GeometryUtility.TestPlanesAABB(cameraPlanes, bounds);
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
        float angleThreshold = 0.5f; // Пороговое значение угла (примерно 0.99 соответствует 8 градусам)
        return dotProduct <= angleThreshold;
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