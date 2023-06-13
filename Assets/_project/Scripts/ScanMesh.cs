using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private Renderer _quadRenderer;

    private List<MeshRenderer> _meshes = new List<MeshRenderer>();

    private Texture2D _cameraTexture;

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
        //_arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
        //_arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnDestroy()
    {
        Destroy(_cameraTexture);
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

        Debug.Log($"vertices: {vertices}. Length: {vertices.Length}");
        // Создание текстурных координат для меша
        Vector2[] uvs = GenerateUVs(vertices);

        GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        // Передача данных меша объекту
        Mesh meshComponent = new Mesh();
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshObject.GetComponent<MeshFilter>().mesh = meshComponent;

        Debug.Log($"Coords: {meshComponent.uv}. Length: {meshComponent.uv.Length}");
        // Расположение объекта в пространстве
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;
        var meshRenderer = meshObject.GetComponent<MeshRenderer>();
        _meshes.Add(meshObject.GetComponent<MeshRenderer>());

        //CutTexture(meshComponent, meshRenderer);

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
        //ApplyCameraTextureToMesh(meshObject, meshFilter);
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

    private Texture2D ColorMeshWithCameraTexture(MeshFilter meshFilter, Texture2D cameraTexture)
    {
        // Получаем меш и его размеры
        Mesh mesh = meshFilter.sharedMesh;
        Bounds meshBounds = mesh.bounds;
        Vector3 meshSize = meshBounds.size;

        Debug.Log($"Size: {meshSize.x} : {meshSize.y}");
        // Создаем новую текстуру с размерами меша
        Texture2D meshTexture = new Texture2D((int)meshSize.x, (int)meshSize.y, TextureFormat.RGBA32, false);

        // Получаем вершины меша в локальных координатах
        Vector3[] meshVertices = mesh.vertices;

        // Проходим по вершинам и преобразуем их в координаты текстуры
        Vector2[] meshUVs = new Vector2[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
        {
            Vector3 vertex = meshVertices[i];
            Vector3 normalizedVertex = new Vector3(
                (vertex.x - meshBounds.min.x) / meshSize.x,
                (vertex.y - meshBounds.min.y) / meshSize.y,
                (vertex.z - meshBounds.min.z) / meshSize.z
            );
            meshUVs[i] = new Vector2(normalizedVertex.x, normalizedVertex.y);
        }

        // Проходим по треугольникам меша и копируем пиксели из соответствующих областей на кадре в текстуру меша
        int[] meshTriangles = mesh.triangles;
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            Vector2 uv1 = meshUVs[meshTriangles[i]];
            Vector2 uv2 = meshUVs[meshTriangles[i + 1]];
            Vector2 uv3 = meshUVs[meshTriangles[i + 2]];

            int minX = Mathf.FloorToInt(Mathf.Min(uv1.x, uv2.x, uv3.x) * cameraTexture.width);
            int minY = Mathf.FloorToInt(Mathf.Min(uv1.y, uv2.y, uv3.y) * cameraTexture.height);
            int maxX = Mathf.CeilToInt(Mathf.Max(uv1.x, uv2.x, uv3.x) * cameraTexture.width);
            int maxY = Mathf.CeilToInt(Mathf.Max(uv1.y, uv2.y, uv3.y) * cameraTexture.height);

            Color[] pixels = cameraTexture.GetPixels(minX, minY, maxX - minX, maxY - minY);

            // Получаем индексы вершин треугольника в массиве треугольников
            int index1 = meshTriangles[i];
            int index2 = meshTriangles[i + 1];
            int index3 = meshTriangles[i + 2];

            // Задаем UV-координаты для треугольника
            meshUVs[index1] = new Vector2((uv1.x - minX) / (maxX - minX), (uv1.y - minY) / (maxY - minY));
            meshUVs[index2] = new Vector2((uv2.x - minX) / (maxX - minX), (uv2.y - minY) / (maxY - minY));
            meshUVs[index3] = new Vector2((uv3.x - minX) / (maxX - minX), (uv3.y - minY) / (maxY - minY));

            // Задаем цвета вершин треугольника на текстуре меша
            meshTexture.SetPixels((int)meshUVs[index1].x, (int)meshUVs[index1].y, 1, 1, pixels);
            meshTexture.SetPixels((int)meshUVs[index2].x, (int)meshUVs[index2].y, 1, 1, pixels);
            meshTexture.SetPixels((int)meshUVs[index3].x, (int)meshUVs[index3].y, 1, 1, pixels);
        }

        // Применяем изменения на текстуре меша
        meshTexture.Apply();

        // Устанавливаем текстуру меша в материале
        return meshTexture;
    }

    private void CutTexture(Mesh mesh, MeshRenderer meshRenderer)
    {
        var uv = mesh.uv;
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        int numVertices = mesh.vertexCount;
        for (int i = 0; i < numVertices; i++)
        {
            Vector2 uvCoordinate = uv[i];
            minX = Mathf.Min(minX, uvCoordinate.x);
            minY = Mathf.Min(minY, uvCoordinate.y);
            maxX = Mathf.Max(maxX, uvCoordinate.x);
            maxY = Mathf.Max(maxY, uvCoordinate.y);
        }

        // Преобразуем координаты в пиксельные координаты
        int textureWidth = _cameraTexture.width;
        int textureHeight = _cameraTexture.height;

        int pixelMinX = Mathf.FloorToInt(minX * textureWidth);
        int pixelMinY = Mathf.FloorToInt(minY * textureHeight);
        int pixelMaxX = Mathf.FloorToInt(maxX * textureWidth);
        int pixelMaxY = Mathf.FloorToInt(maxY * textureHeight);

        int width = pixelMaxX - pixelMinX + 1;
        int height = pixelMaxY - pixelMinY + 1;

        Texture2D meshTexture = new Texture2D(width, height);

        Color[] meshPixels = _cameraTexture.GetPixels(pixelMinX, pixelMinY, width, height);
        meshTexture.SetPixels(meshPixels);
        meshTexture.Apply();


        // Создаем новый материал для меша
        Material material = new Material(Shader.Find("Standard"));

        // Присваиваем текстуру новому материалу
        material.mainTexture = meshTexture;

        // Применяем новый материал к мешу
        meshRenderer.material = material;
    }

    private void ApplyCameraTextureToMesh(GameObject meshObject, MeshFilter meshFilter)
    {
        ToogleMeshes(false);
        var cameraTexture = GetCameraTexture();

        Texture2D rotatedTexture = RotateTexture(cameraTexture, true);
        rawImage.texture = rotatedTexture;
        _quadRenderer.sharedMaterial.mainTexture = rotatedTexture;

        ToogleMeshes(true);
        return;
        if (_arCameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            var texture = ColorMeshWithCameraTexture(meshObject.GetComponent<MeshFilter>(), _cameraTexture);
            cpuImage.Dispose();

            rawImage.texture = texture;

            if (texture != null)
            {
                // Создаем новый материал для меша
                Material material = new Material(Shader.Find("Standard"));

                // Присваиваем текстуру новому материалу
                material.mainTexture = texture;

                //// Изменяем ориентацию текстуры
                //material.mainTextureScale = new Vector2(-1, 1); // Изменяем знаки X-координаты и Y-координаты

                // Применяем новый материал к мешу
                meshObject.GetComponent<MeshRenderer>().material = material;
            }
        }
        ToogleMeshes(true);
    }

    private Texture2D GetCameraTexture()
    {
        if (_arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // Вычисляем размеры области кадра, соответствующей мешу
            // Определяем размеры области кадра, соответствующей мешу
            int imageWidth = image.width;
            int imageHeight = image.height;

            // Создаем новую текстуру с размерами меша
            Texture2D cameraTexture = new Texture2D(imageWidth, imageHeight, image.format.AsTextureFormat(), false);

            // Определяем размер буфера для преобразования
            int bufferSize = image.GetConvertedDataSize(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, imageWidth, imageHeight),
                outputDimensions = new Vector2Int(imageWidth, imageHeight),
                outputFormat = TextureFormat.RGBA32
            });

            // Создаем буфер для преобразования
            NativeArray<byte> buffer = new NativeArray<byte>(bufferSize, Allocator.Temp);

            // Выполняем преобразование
            image.Convert(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, imageWidth, imageHeight),
                outputDimensions = new Vector2Int(imageWidth, imageHeight),
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

    //private Texture2D RotateTextureClockwise(Texture2D texture)
    //{
    //    int width = texture.width;
    //    int height = texture.height;

    //    // Создаем новую текстуру с измененными размерами
    //    Texture2D rotatedTexture = new Texture2D(height, width, TextureFormat.RGBA32, false);

    //    // Получаем пиксели из исходной текстуры
    //    Color[] pixels = texture.GetPixels();

    //    // Поворачиваем пиксели по часовой стрелке
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            rotatedTexture.SetPixel(y, width - x - 1, pixels[x + y * width]);
    //        }
    //    }

    //    // Применяем изменения к повернутой текстуре
    //    rotatedTexture.Apply();

    //    return rotatedTexture;
    //}

    private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Debug.Log("Start Rotate Texture");
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();

        Debug.Log("End Rotate Texture");
        return rotatedTexture;
    }

    private void ToogleMeshes(bool activate)
    {
        rawImage.enabled = activate;
        if (_meshes == null || _meshes.Count <= 0)
            return;

        _meshes.ForEach(mesh =>
        {
            if (mesh != null)
                mesh.enabled = activate;
        });
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