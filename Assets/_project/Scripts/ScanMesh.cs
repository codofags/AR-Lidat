using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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
    [SerializeField] private Texture2D _testTexute;
    

    public class ScanMeshInfo
    {
        public MeshRenderer MeshRenderer;
        public Texture2D Texture;

        public ScanMeshInfo(MeshRenderer meshRenderer, Texture2D texture)
        {
            MeshRenderer = meshRenderer;
            Texture = texture;
        }
    }

    private List<ScanMeshInfo> _scans = new List<ScanMeshInfo>();
    private List<MeshRenderer> _meshes = new List<MeshRenderer>();

    private bool _test;

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

    private void Update()
    {
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

        //Debug.Log($"vertices: {vertices}. Length: {vertices.Length}");
        //// Создание текстурных координат для меша
        //Vector2[] uvs = GenerateUVs(vertices);



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

        if (_test)
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


        if (_test)
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

    public void GetAndSaveMesh(MeshRenderer meshRenderer)
    {
        var cameraTexture = GetCameraTexture();

        cameraTexture.RotateTexture(true);
        cameraTexture.FlipTexture();
        ScanMeshInfo mesh = new ScanMeshInfo(meshRenderer, cameraTexture);
        _scans.Add(mesh);
    }

    private void ApplyCameraTextureToMesh(MeshFilter meshFilter)
    {
        //ToogleMeshes(false);

        if (_arCameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            var cameraTexture = GetCameraTexture();

            CalculatePlanarUV(meshFilter.mesh);
            var uvs = meshFilter.mesh.uv;
            meshFilter.mesh.uv = uvs;

            cameraTexture.RotateTexture(true);
            cameraTexture.FlipTexture();

            var color = CreateArrayPixelColor(meshFilter.mesh.vertices, meshFilter.mesh.uv, cameraTexture);
            meshFilter.mesh.colors = color;
            //ColorsFromPixel(meshFilter.mesh, cameraTexture);

            //var uvs = GetTextureCoordForVertices(meshFilter.mesh, cameraTexture);
            //var colors = CreateArrayPixelColor(meshFilter.mesh.vertices, uvs, cameraTexture);

            //meshFilter.mesh.colors = colors;

            //for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
            //{
            //    Vector3 vertex = meshFilter.mesh.vertices[i];
            //    Vector3 normal = meshFilter.mesh.normals[i / 3]; // Получаем нормаль треугольника

            //    // Проецируем вершину на текстуру, используя нормаль треугольника
            //    Vector2 projectedUV = ProjectVertexToTexture(vertex, normal, cameraTexture);

            //    // Применяем полученные текстурные координаты (UV)
            //    uvs[i] = projectedUV;
            //}

            //if (_test)
            //{

            //    // Создаем новый материал для меша
            //    Material material = new Material(Shader.Find("Standard"));

            //    // Присваиваем текстуру новому материалу
            //    material.mainTexture = cameraTexture;

            //    // Применяем новый материал к мешу
            //    meshFilter.GetComponent<MeshRenderer>().material = material;
            //}

            // Обрезаем кадр камеры в соответствии с мешем
            //Texture2D croppedTexture = CropCameraFrameWithMesh(cameraTexture, meshFilter);
            cpuImage.Dispose();
            var texture = cameraTexture;
            rawImageCut.texture = texture;
        }
    }


    void CalculatePlanarUV(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = new Vector2[vertices.Length];
        var normals = mesh.normals;

        // Выбираем направление проекции текстуры (например, вдоль оси X)
        Vector3 textureProjectionDirection = Vector3.zero;

        // Выбираем наиболее распространенную направленность нормалей
        for (int i = 0; i < normals.Length; i++)
        {
            if (normals[i].sqrMagnitude > textureProjectionDirection.sqrMagnitude)
            {
                textureProjectionDirection = normals[i];
            }
        }
        textureProjectionDirection = Vector3.down;

        // Нормализуем направление проекции текстуры
        textureProjectionDirection.Normalize();

        // Проходим по всем вершинам и вычисляем текстурные координаты
        for (int i = 0; i < vertices.Length; i++)
        {
            // Проецируем вершину на плоскость, перпендикулярную выбранному направлению
            Vector2 planarPosition = new Vector2(vertices[i].x, vertices[i].y);

            // Устанавливаем текстурную координату на основе позиции вершины
            uv[i] = new Vector2(Vector2.Dot(planarPosition, textureProjectionDirection), vertices[i].z);
        }

        // Присваиваем массив текстурных координат мешу
        mesh.uv = uv;
    }

    Texture2D CropCameraFrameWithMesh(Texture2D cameraFrame, MeshFilter meshFilter)
    {
        // Получаем размеры кадра камеры
        int width = cameraFrame.width;
        int height = cameraFrame.height;

        // Получаем вершины, треугольники и нормали меша
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = mesh.normals;

        // Создаем новую текстуру для обрезанного кадра
        Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Проходимся по каждому пикселю кадра и проверяем, находится ли он внутри меша
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pixelPosition = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 1f));

        if (IsPixelInsideMesh(pixelPosition, vertices, triangles, normals))
        {
            Color pixelColor = cameraFrame.GetPixel(x, y);
            croppedTexture.SetPixel(x, y, pixelColor);
        }
        else
        {
            croppedTexture.SetPixel(x, y, Color.clear);
        }
            }
        }

        // Применяем изменения к обрезанной текстуре
        croppedTexture.Apply();

        return croppedTexture;
    }

    bool IsPixelInsideMesh(Vector3 pixelPosition, Vector3[] vertices, int[] triangles, Vector3[] normals)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 vertex1 = vertices[triangles[i]];

            Vector3 triangleNormal = normals[i / 3]; // Получаем нормаль треугольника

            // Проверяем, находится ли пиксель по ту сторону треугольника, где находится нормаль
            if (Vector3.Dot(pixelPosition - vertex1, triangleNormal) > 0)
            {
                return false; // Пиксель находится за границами меша
            }
        }

        return true; // Пиксель находится внутри меша
    }

    Vector2 ProjectVertexToTexture(Vector3 vertex, Vector3 normal, Texture2D texture)
    {
        // Преобразование 3D-координат вершины в 2D-координаты текстуры
        Vector3 projectedPoint = Camera.main.WorldToScreenPoint(vertex);
        Vector2 projectedUV = new Vector2(projectedPoint.x / Screen.width, projectedPoint.y / Screen.height);

        // Применение полученных 2D-координат к размерам текстуры
        projectedUV.x *= texture.width;
        projectedUV.y *= texture.height;

        return projectedUV;
    }


    Color32 GetPixelColor(byte[] pixelData, int x, int y, int width, int height)
    {
        int index = (y * width + x) * 4;

        byte r = pixelData[index];
        byte g = pixelData[index + 1];
        byte b = pixelData[index + 2];
        byte a = pixelData[index + 3];

        return new Color32(r, g, b, a);
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

    private Vector2 GetUVFromWorldPosition(Vector3 worldPosition, Texture2D frameTexture)
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

        // Получаем текстурные координаты из цвета пикселя, нормализуя значения в диапазоне [0, 1]
        float uNormalized = pixelColor.r / 255.0f;
        float vNormalized = pixelColor.g / 255.0f;
        Vector2 uv = new Vector2(uNormalized, vNormalized);

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

    void ColorsFromPixel(Mesh mesh, Texture2D texture)
    {
        var vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertexWorldPos = transform.TransformPoint(vertices[i]);
            Vector3 viewportPos = _arCameraManager.GetComponent<Camera>().WorldToViewportPoint(vertexWorldPos);
            Vector2 pixelUV = new Vector2(viewportPos.x * texture.width, viewportPos.y * texture.height);
            Color pixelColor = texture.GetPixel(Mathf.FloorToInt(pixelUV.x), Mathf.FloorToInt(pixelUV.y));

            colors[i] = pixelColor;
        }

        mesh.colors = colors;
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

    public void test()
    {
        _arCameraManager.enabled = !_arCameraManager.enabled;
    }

    public void test_s()
    {
        _test = !_test;
    }
}