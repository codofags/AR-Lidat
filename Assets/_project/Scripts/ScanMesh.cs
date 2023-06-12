using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private RawImage _fullRawImage;

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            CreateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            UpdateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            RemoveMeshObject(meshFilter);
        }
    }

    private void CreateMeshObject(MeshFilter meshFilter)
    {
        // Получение вершин меша
        Vector3[] vertices = meshFilter.mesh.vertices;

        // Получение треугольников меша
        int[] triangles = meshFilter.mesh.triangles;

        // Создание текстурных координат для меша
        Vector2[] uvs = GenerateUVs(vertices);

        GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        // Передача данных меша объекту
        Mesh meshComponent = new Mesh();
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshObject.GetComponent<MeshFilter>().mesh = meshComponent;

        // Расположение объекта в пространстве
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;

        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshObject, meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        // Получение вершин меша
        Vector3[] vertices = meshFilter.sharedMesh.vertices;

        // Получение треугольников меша
        int[] triangles = meshFilter.sharedMesh.triangles;

        // Получение текстурных координат меша
        Vector2[] uvs = meshFilter.sharedMesh.uv;

        // Обновление существующего меша
        GameObject meshObject = meshFilter.gameObject;
        Mesh meshComponent = meshObject.GetComponent<MeshFilter>().mesh;
        meshComponent.Clear(); // Очищаем меш перед обновлением
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshComponent.RecalculateNormals(); // Пересчитываем нормали

        // Расположение объекта в пространстве (если требуется)
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;

        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshObject, meshFilter);
    }

    private void RemoveMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        Destroy(meshFilter.gameObject);
    }

    private Texture2D GetCameraTextureForMesh(MeshFilter meshFilter)
    {
        if (_arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // Получаем ограничивающий прямоугольник меша в мировых координатах
            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Vector3[] meshCorners = new Vector3[8];
            meshCorners[0] = meshBounds.min;
            meshCorners[1] = new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.max.z);
            meshCorners[2] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.min.z);
            meshCorners[3] = new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z);
            meshCorners[4] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.min.z);
            meshCorners[5] = new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.max.z);
            meshCorners[6] = new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.min.z);
            meshCorners[7] = meshBounds.max;

            Vector3 minWorldPoint = meshFilter.transform.TransformPoint(meshCorners[0]);
            Vector3 maxWorldPoint = meshFilter.transform.TransformPoint(meshCorners[7]);

            // Получаем текущую активную камеру
            Camera activeCamera = Camera.main;

            // Преобразуем мировые точки в экранные точки
            Vector3 minScreenPoint = activeCamera.WorldToScreenPoint(minWorldPoint);
            Vector3 maxScreenPoint = activeCamera.WorldToScreenPoint(maxWorldPoint);

            // Преобразуем экранные точки в локальные точки внутри RawImage
            RectTransform rawImageRectTransform = _fullRawImage.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, minScreenPoint, activeCamera, out Vector2 minLocalPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, maxScreenPoint, activeCamera, out Vector2 maxLocalPoint);

            // Вычисляем размеры прямоугольника в пикселях
            float width = Mathf.Abs(maxLocalPoint.x - minLocalPoint.x);
            float height = Mathf.Abs(maxLocalPoint.y - minLocalPoint.y);

            // Создаем прямоугольник для обрезки кадра
            Rect croppedRect = new Rect(minLocalPoint.x, minLocalPoint.y, width, height);

            // Выполняем обрезку кадра с использованием созданного прямоугольника
            Texture2D cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            cameraTexture.LoadRawTextureData(image.GetPlane(0).data);
            cameraTexture.Apply();
            cameraTexture = cameraTexture.CropTexture(croppedRect);

            // Освобождаем ресурсы
            image.Dispose();

            return cameraTexture;
        }

        return null;
    }

    private void ApplyCameraTextureToMesh(GameObject meshObject, MeshFilter meshFilter)
    {
        Texture2D cameraTexture = GetCameraTextureForMesh(meshObject.GetComponent<MeshFilter>());
        rawImage.texture = cameraTexture;
        
        if (cameraTexture != null)
        {
            // Создаем новый материал для меша
            Material material = new Material(Shader.Find("Standard"));

            // Присваиваем текстуру новому материалу
            material.mainTexture = cameraTexture;

            // Изменяем ориентацию текстуры
            material.mainTextureScale = new Vector2(-1, 1); // Изменяем знаки X-координаты и Y-координаты

            // Применяем новый материал к мешу
            meshObject.GetComponent<MeshRenderer>().material = material;
        }
    }

    private Vector2[] GenerateUVs(Vector3[] vertices)
    {
        // Создаем массив текстурных координат для каждой вершины меша
        Vector2[] uvs = new Vector2[vertices.Length];

        // Проходим по каждой вершине и создаем текстурную координату,
        // основанную на позиции вершины в пространстве
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector2 uv = new Vector2(vertex.x, vertex.z);
            uvs[i] = uv;
        }

        return uvs;
    }
}
