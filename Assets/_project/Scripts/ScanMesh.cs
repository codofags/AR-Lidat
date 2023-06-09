using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARCameraManager _cameraManager;

    private ARMeshManager _arMeshManager;
    private AROcclusionManager _occlusionManager;
    private Texture2D _occlusionTexture;

    private void Awake()
    {
        _arMeshManager = GetComponent<ARMeshManager>();
        _occlusionManager = GetComponent<AROcclusionManager>();
    }

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
        _cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
        _cameraManager.frameReceived -= OnCameraFrameReceived;
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

        // Получение текстурных координат меша
        Vector2[] uvs = meshFilter.mesh.uv;

        // Создание текстуры отсканированной области
        Texture2D occlusionTexture = CreateOcclusionTexture();

        GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        // Передача данных меша объекту
        Mesh meshComponent = new Mesh();
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshObject.GetComponent<MeshFilter>().mesh = meshComponent;
        meshObject.GetComponent<MeshRenderer>().material = _meshPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        meshObject.GetComponent<MeshRenderer>().material.SetTexture("_OcclusionMap", occlusionTexture); // Применение текстуры отсканированной области


        // Расположение объекта в пространстве
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;
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
        meshComponent.uv = uvs; // Передача текстурных
        meshComponent.RecalculateNormals(); // Пересчитываем нормали

        // Передача данных обновленного меша объекту
        meshObject.GetComponent<MeshRenderer>().sharedMaterial = meshFilter.GetComponent<MeshRenderer>().sharedMaterial;

        // Расположение объекта в пространстве (если требуется)
        meshObject.transform.position = meshFilter.transform.position;
        meshObject.transform.rotation = meshFilter.transform.rotation;
        meshObject.transform.localScale = Vector3.one;
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

    private Texture2D CreateOcclusionTexture()
    {
        if (_occlusionManager == null || !_occlusionManager.enabled)
        {
            Debug.LogError("AROcclusionManager is not available or not enabled.");
            return null;
        }

        if (_occlusionTexture == null)
        {
            int textureWidth = _occlusionTexture.width;
            int textureHeight = _occlusionTexture.height;

            _occlusionTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        }

        return _occlusionTexture;
    }

    //private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    //{
    //    if (_occlusionTexture != null)
    //    {
    //        if (eventArgs.textures.TryGetValue(_occlusionManager.currentEnvironmentDepthMipLevel, out var occlusionTexture))
    //        {
    //            Graphics.CopyTexture(occlusionTexture, _occlusionTexture);
    //            _occlusionTexture.Apply();
    //        }
    //    }
    //}

    //private Texture2D CreateOcclusionTexture()
    //{
    //    // Получение информации об отсканированной текстуре из AROcclusionManager
    //    ARTextureInfo occlusionTextureInfo = _occlusionManager.environmentDepthTexture;

    //    if (occlusionTextureInfo.descriptor.dimension != TextureDimension.Tex2D)
    //    {
    //        Debug.LogError("Occlusion texture is not a 2D texture.");
    //        return null;
    //    }

    //    // Создание новой Texture2D для отсканированной области
    //    Texture2D occlusionTexture = new Texture2D(occlusionTextureInfo.descriptor.width, occlusionTextureInfo.descriptor.height, TextureFormat.RGBA32, false);

    //    // Обновление внешней текстуры с помощью указателя на нативную текстуру
    //    occlusionTexture.UpdateExternalTexture(occlusionTextureInfo.GetNativeTexturePtr());

    //    // Освобождение ресурсов, связанных с текстурой
    //    _occlusionManager.ReleaseEnvironmentDepthTexture();

    //    return occlusionTexture;
    //}

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (_occlusionTexture != null)
        {
            _occlusionTexture.UpdateExternalTexture(eventArgs.textures[0].GetNativeTexturePtr());
        }
    }
}

