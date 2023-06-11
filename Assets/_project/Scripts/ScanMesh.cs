﻿using System.Collections.Generic;
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
            // Получаем размеры меша
            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Vector3 meshSize = meshBounds.size;

            // Преобразуем размеры меша в нормализованные координаты от 0 до 1
            float normalizedWidth = meshSize.x / meshBounds.extents.x;
            float normalizedHeight = meshSize.z / meshBounds.extents.z;

            // Получаем прямоугольник, соответствующий размерам меша
            Rect meshRect = new Rect(0, 0, normalizedWidth, normalizedHeight);

            // Вычисляем размеры области кадра, соответствующей мешу
            // Определяем размеры области кадра, соответствующей мешу
            int imageWidth = image.width;
            int imageHeight = image.height;
            int x = Mathf.FloorToInt(meshRect.x * image.width);
            int y = Mathf.FloorToInt(meshRect.y * image.height);
            int width = Mathf.CeilToInt(meshRect.width * image.width);
            int height = Mathf.CeilToInt(meshRect.height * image.height);

            // Проверяем, чтобы размеры преобразованного изображения не превышали размеры оригинального изображения
            if (x + width > imageWidth)
            {
                width = imageWidth - x;
            }

            if (y + height > imageHeight)
            {
                height = imageHeight - y;
            }

            // Создаем новую текстуру с размерами меша
            Texture2D cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Определяем размер буфера для преобразования
            int bufferSize = image.GetConvertedDataSize(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(x, y, width, height),
                outputDimensions = new Vector2Int(width, height),
                outputFormat = TextureFormat.RGBA32
            });

            // Создаем буфер для преобразования
            NativeArray<byte> buffer = new NativeArray<byte>(bufferSize, Allocator.Temp);

            // Выполняем преобразование
            image.Convert(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(x, y, width, height),
                outputDimensions = new Vector2Int(width, height),
                outputFormat = TextureFormat.RGBA32
            }, buffer);

            // Копируем данные из буфера в текстуру
            cameraTexture.LoadRawTextureData(buffer);
            cameraTexture.Apply();

            // Освобождаем ресурсы
            image.Dispose();
            buffer.Dispose();

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

            // Применяем новый материал к мешу
            meshObject.GetComponent<MeshRenderer>().material = material;
        }
    }


    private void ApplyTextureToMesh(GameObject meshObject, Texture2D texture)
    {
        if (meshObject != null && texture != null)
        {
            // Создаем новый материал для меша
            Material material = new Material(Shader.Find("Standard"));

            // Присваиваем текстуру новому материалу
            material.mainTexture = texture;

            // Применяем новый материал к мешу
            MeshRenderer meshRenderer = meshObject.GetComponent<MeshRenderer>();
            meshRenderer.material = material;
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