using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private RawImage rawImageCut;
    [SerializeField] private Renderer _quadRenderer;

    private List<MeshRenderer> _meshes = new List<MeshRenderer>();

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


        foreach (var vertice in vertices)
        {
            Debug.Log($"Vert_x: {vertice.x} - Vert_y: {vertice.y} - Vert_z: {vertice.z}");
        }

        // Получение треугольников меша
        int[] triangles = meshFilter.mesh.triangles;

        foreach (var triangle in triangles)
        {
            Debug.Log($"Triangle: {triangle}");
        }
        //Debug.Log($"vertices: {vertices}. Length: {vertices.Length}");
        //// Создание текстурных координат для меша
        Vector2[] uvs = GenerateUVs(vertices);


        foreach (var uv in uvs)
        {
            Debug.Log($"UV_x: {uv.x} - UV_y: {uv.y}");
        }



        //GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        //// Передача данных меша объекту
        //Mesh meshComponent = new Mesh();
        //meshComponent.vertices = vertices;
        //meshComponent.triangles = triangles;
        //meshComponent.uv = uvs; // Передача текстурных координат
        //meshObject.GetComponent<MeshFilter>().mesh = meshComponent;

        //Debug.Log($"Coords: {meshComponent.uv}. Length: {meshComponent.uv.Length}");
        //// Расположение объекта в пространстве
        //meshObject.transform.position = meshFilter.transform.position;
        //meshObject.transform.rotation = meshFilter.transform.rotation;
        //meshObject.transform.localScale = Vector3.one;
        //var meshRenderer = meshObject.GetComponent<MeshRenderer>();
        _meshes.Add(meshFilter.GetComponent<MeshRenderer>());

        //CutTexture(meshComponent, meshRenderer);

        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        //// Получение вершин меша
        //Vector3[] vertices = meshFilter.sharedMesh.vertices;

        //// Получение треугольников меша
        //int[] triangles = meshFilter.sharedMesh.triangles;

        //// Получение текстурных координат меша
        //Vector2[] uvs = meshFilter.sharedMesh.uv;

        //// Обновление существующего меша
        //GameObject meshObject = meshFilter.gameObject;
        //Mesh meshComponent = meshObject.GetComponent<MeshFilter>().mesh;
        //meshComponent.Clear(); // Очищаем меш перед обновлением
        //meshComponent.vertices = vertices;
        //meshComponent.triangles = triangles;
        //meshComponent.uv = uvs; // Передача текстурных координат
        //meshComponent.RecalculateNormals(); // Пересчитываем нормали

        //// Расположение объекта в пространстве (если требуется)
        //meshObject.transform.position = meshFilter.transform.position;
        //meshObject.transform.rotation = meshFilter.transform.rotation;
        //meshObject.transform.localScale = Vector3.one;

        // Применяем текстуру к мешу
        ApplyCameraTextureToMesh(meshFilter);
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

    private Texture2D CutTexture(MeshFilter meshFilter, Texture2D texture)
    {
        Debug.Log("Start Cut");
        var uv = meshFilter.sharedMesh.uv;
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        int numVertices = meshFilter.sharedMesh.vertexCount;
        for (int i = 0; i < numVertices; i++)
        {
            Vector2 uvCoordinate = uv[i];
            minX = Mathf.Min(minX, uvCoordinate.x);
            minY = Mathf.Min(minY, uvCoordinate.y);
            maxX = Mathf.Max(maxX, uvCoordinate.x);
            maxY = Mathf.Max(maxY, uvCoordinate.y);
        }

        // Преобразуем координаты в пиксельные координаты
        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int pixelMinX = Mathf.FloorToInt(minX * textureWidth);
        int pixelMinY = Mathf.FloorToInt(minY * textureHeight);
        int pixelMaxX = Mathf.FloorToInt(maxX * textureWidth);
        int pixelMaxY = Mathf.FloorToInt(maxY * textureHeight);

        int width = pixelMaxX - pixelMinX + 1;
        int height = pixelMaxY - pixelMinY + 1;

        Texture2D meshTexture = new Texture2D(width, height);

        Color[] meshPixels = texture.GetPixels(pixelMinX, pixelMinY, width, height);
        meshTexture.SetPixels(meshPixels);
        meshTexture.Apply();
        Debug.Log("End Cut");
        return meshTexture;
    }


    private void ApplyCameraTextureToMesh(MeshFilter meshFilter)
    {
        ToogleMeshes(false);

        if (_arCameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            var cameraTexture = GetCameraTexture();

            cameraTexture = RotateTexture(cameraTexture, true);
            cameraTexture = FlipTexture(cameraTexture);

            var uvs = GetTextureCoordForVertices(meshFilter.mesh, cameraTexture);

            foreach (var uv in uvs)
            {
                Debug.Log($"Texture coord_x: {uv.x} - Texture coord_y: {uv.y}");
            }

            var colors = CreateArrayPixelColor(meshFilter.mesh.vertices, uvs, cameraTexture);

            meshFilter.mesh.colors = colors;
            cpuImage.Dispose();

            var texture = cameraTexture;
            _quadRenderer.sharedMaterial.mainTexture = texture;
            rawImage.texture = cameraTexture;
            rawImageCut.texture = texture;
            //if (texture != null)
            //{
            //    // Создаем новый материал для меша
            //    Material material = new Material(Shader.Find("Standard"));

            //    // Присваиваем текстуру новому материалу
            //    material.mainTexture = texture;

            //    //// Изменяем ориентацию текстуры
            //    //material.mainTextureScale = new Vector2(-1, 1); // Изменяем знаки X-координаты и Y-координаты

            //    // Применяем новый материал к мешу
            //    meshFilter.GetComponent<MeshRenderer>().material = material;
            //}
        }
        ToogleMeshes(true);
    }

    private Vector2[] GetTextureCoordForVertices(Mesh mesh, Texture2D frameTexture)
    {
        var vertices = mesh.vertices;

        // Получаем текстурные координаты для каждой вершины меша
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = GetUVFromWorldPosition(vertices[i], frameTexture);
        }

        return uv;
    }

    private Color[] CreateArrayPixelColor(Vector3[] vertices, Vector2[] uvs, Texture2D frameTexture)
    {
        // Создаем массив цветов пикселей для вершин меша
        Color[] vertexColors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 uvCoord = uvs[i];

            // Получаем цвет пикселя из текстуры, используя текстурные координаты
            Color pixelColor = frameTexture.GetPixelBilinear(uvCoord.x, uvCoord.y);

            // Назначаем цвет пикселя вершине меша
            vertexColors[i] = pixelColor;
        }
        return vertexColors;
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
            Texture2D cameraTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);

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

    private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
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
        return rotatedTexture;
    }

    private Texture2D FlipTexture(Texture2D originalTexture)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] flipped = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iFlipped, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iFlipped = j * w + (w - i - 1);
                iOriginal = j * w + i;
                flipped[iFlipped] = original[iOriginal];
            }
        }

        Texture2D flippedTexture = new Texture2D(w, h);
        flippedTexture.SetPixels32(flipped);
        flippedTexture.Apply();
        return flippedTexture;
    }

    private Color[] GetMeshPixelColors(MeshFilter meshFilter, Texture2D cameraTexture)
    {
        Debug.Log("Start Get Color Pixels");
        Mesh mesh = meshFilter.sharedMesh;
        Bounds meshBounds = mesh.bounds;
        Vector3 meshSize = meshBounds.size;

        // Получаем вершины меша в локальных координатах
        Vector3[] meshVertices = mesh.vertices;

        // Создаем массив цветов пикселей
        Color[] pixelColors = new Color[meshVertices.Length];

        for (int i = 0; i < meshVertices.Length; i++)
        {
            // Преобразуем вершину меша в мировые координаты
            Vector3 worldVertex = meshFilter.transform.TransformPoint(meshVertices[i]);

            // Преобразуем мировые координаты в локальные координаты текстуры меша
            Vector2 meshUV = new Vector2(
                (worldVertex.x - meshBounds.min.x) / meshSize.x,
                (worldVertex.y - meshBounds.min.y) / meshSize.y
            );

            // Преобразуем локальные координаты текстуры меша в координаты текстуры кадра
            Vector2 textureCoord = new Vector2(
                meshUV.x * cameraTexture.width,
                meshUV.y * cameraTexture.height
            );

            // Получаем цвет пикселя по координатам текстуры
            pixelColors[i] = cameraTexture.GetPixel((int)textureCoord.x, (int)textureCoord.y);
        }

        Debug.Log("End Get Color Pixels");
        return pixelColors;
    }

    private Texture2D CreateTextureFromColors(Color[] pixelColors, int width, int height)
    {
        Debug.Log("Start Create Colored Texture");
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixelColors);
        texture.Apply();
        Debug.Log("End Create Colored Texture");
        return texture;
    }

    Vector2 GetUVFromWorldPosition(Vector3 worldPosition, Texture2D frameTexture)
    {
        // Получаем размеры текстуры кадра
        int textureWidth = frameTexture.width;
        int textureHeight = frameTexture.height;

        // Конвертируем мировые координаты в экранные координаты
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Преобразуем экранные координаты в текстурные координаты
        float u = screenPosition.x / Screen.width;
        float v = screenPosition.y / Screen.height;

        // Преобразуем текстурные координаты в координаты текстуры кадра
        int x = Mathf.FloorToInt(u * textureWidth);
        int y = Mathf.FloorToInt(v * textureHeight);

        // Получаем цвет пикселя из текстуры кадра
        Color32 pixelColor = frameTexture.GetPixel(x, y);

        // Получаем текстурные координаты из цвета пикселя
        Vector2 uv = new Vector2(pixelColor.r, pixelColor.g);

        return uv;
    }

    Color32 GetPixelColorFromTexture(int x, int y, int textureWidth, int textureHeight, IntPtr textureY, IntPtr textureCbCr)
    {
        // Получаем индекс пикселя в массиве данных текстуры
        int pixelIndex = y * textureWidth + x;

        // Получаем значения компонент цвета из массивов данных текстуры
        byte yValue = Marshal.ReadByte(textureY, pixelIndex);
        byte cbValue = Marshal.ReadByte(textureCbCr, pixelIndex);
        byte crValue = Marshal.ReadByte(textureCbCr, pixelIndex + 1);

        // Преобразуем компоненты цвета YCbCr в RGB
        float yf = (float)yValue / 255.0f;
        float cb = (float)cbValue / 255.0f - 0.5f;
        float cr = (float)crValue / 255.0f - 0.5f;

        float r = yf + 1.402f * cr;
        float g = yf - 0.344f * cb - 0.714f * cr;
        float b = yf + 1.772f * cb;

        // Ограничиваем значения компонентов цвета в диапазоне [0, 1]
        r = Mathf.Clamp01(r);
        g = Mathf.Clamp01(g);
        b = Mathf.Clamp01(b);

        // Преобразуем значения компонентов цвета в диапазон [0, 255]
        byte rByte = (byte)(r * 255);
        byte gByte = (byte)(g * 255);
        byte bByte = (byte)(b * 255);

        return new Color32(rByte, gByte, bByte, 255);
    }

    private Vector2 GetUVFromWorldPoint(Vector3 worldPoint, Texture2D texture)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(worldPoint);
        Vector2 uv = new Vector2(viewportPoint.x * texture.width, viewportPoint.y * texture.height);
        return uv;
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