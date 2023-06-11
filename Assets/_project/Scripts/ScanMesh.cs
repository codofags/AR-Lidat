using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ScanMesh : MonoBehaviour
{
    [SerializeField] private GameObject _meshPrefab; // Префаб для отображения меша
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private AROcclusionManager _occlusionManager;
    [SerializeField] protected RawImage _rawImage;

    private MeshRenderer _meshRenderer;
    private Texture2D _scannedTexture;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
        _occlusionManager.frameReceived += OnOcclusionFrameReceived;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
        _occlusionManager.frameReceived -= OnOcclusionFrameReceived;
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


        GameObject meshObject = Instantiate(_meshPrefab, Vector3.zero, Quaternion.identity);

        // Передача данных меша объекту
        Mesh meshComponent = new Mesh();
        meshComponent.vertices = vertices;
        meshComponent.triangles = triangles;
        meshComponent.uv = uvs; // Передача текстурных координат
        meshObject.GetComponent<MeshFilter>().mesh = meshComponent;

        _meshRenderer = meshObject.GetComponent<MeshRenderer>();

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
        meshComponent.uv = uvs; // Передача текстурных координат
        meshComponent.RecalculateNormals(); // Пересчитываем нормали\


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

    private void OnOcclusionFrameReceived(AROcclusionFrameEventArgs eventArgs)
    {
        if (eventArgs.textures.Count > 0)
        {
            _scannedTexture = eventArgs.textures[0];
            _rawImage.texture = _scannedTexture;
            // Применить текстуру к мешу
            ApplyTextureToMesh(_scannedTexture, _meshRenderer);
        }
    }

    private void ApplyTextureToMesh(Texture2D texture, MeshRenderer meshRenderer)
    {
        if (meshRenderer != null && texture != null)
        {
            // Создаем новый материал для меша
            Material material = new Material(Shader.Find("Standard"));

            // Присваиваем текстуру новому материалу
            material.mainTexture = texture;

            // Применяем новый материал к мешу
            meshRenderer.material = material;
        }
    }
}

