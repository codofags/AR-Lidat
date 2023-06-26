using UnityEngine;
using EzySlice;
using System.Collections.Generic;

public class MeshSlicer : MonoBehaviour
{
    [SerializeField] private float _cubeSize = 10f;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Material _crossSectionMaterial;
    [SerializeField] private MeshFilter _test;

    public List<MeshRenderer> SliceMeshIntoCubes(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            return null;
        }

        List<MeshRenderer> slicedMeshes = new List<MeshRenderer>();
        Mesh mesh = meshFilter.mesh;

        // Определяем границы объекта
        Bounds bounds = mesh.bounds;

        // Размеры куба
        float cubeSize = _cubeSize * 0.01f;

        // Цикл по разделению объекта на кубы
        for (float x = bounds.min.x; x < bounds.max.x; x += cubeSize)
        {
            for (float y = bounds.min.y; y < bounds.max.y; y += cubeSize)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += cubeSize)
                {
                    // Создаем новый куб из префаба
                    GameObject cube = Instantiate(_cubePrefab, new Vector3(x, y, z), Quaternion.identity);

                    // Определяем плоскость разреза
                    Vector3 slicePosition = transform.position;
                    Vector3 sliceDirection = Vector3.up;
                    TextureRegion cuttingRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

                    // Разделяем меш объекта с помощью EzySlice
                    GameObject[] slicedObjects = cube.SliceInstantiate(slicePosition, sliceDirection, cuttingRegion, _crossSectionMaterial);

                    // Удаляем исходный куб
                    Destroy(cube);

                    if (slicedObjects != null)
                    {
                        // Перебираем разделенные объекты и присваиваем им нужные компоненты и материалы
                        foreach (GameObject slicedObject in slicedObjects)
                        {
                            // Добавляем компонент MeshCollider
                            slicedObject.AddComponent<MeshCollider>();

                            // Присваиваем им материал объекта
                            slicedObject.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
                            slicedMeshes.Add(slicedObject.GetComponent<MeshRenderer>());
                        }
                    }
                }
            }
        }

        return slicedMeshes;
    }
    public List<MeshRenderer> SliceMeshesIntoCubes(List<MeshFilter> meshFilters)
    {
        if (meshFilters == null || meshFilters.Count == 0)
        {
            Debug.LogError("MeshFilters list is null or empty!");
            return null;
        }

        List<MeshRenderer> slicedMeshes = new List<MeshRenderer>();

        // Цикл по всем MeshFilter'ам в списке
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter == null)
            {
                Debug.LogError("MeshFilter is null!");
                continue;
            }

            float cubeSize = _cubeSize * 0.01f;
            Mesh mesh = meshFilter.mesh;
            GameObject originalMeshGO = meshFilter.gameObject;
            Material originalMaterial = meshFilter.GetComponent<MeshRenderer>().material;

            // Определяем границы меша
            Bounds bounds = mesh.bounds;

            // Цикл по разделению меша на кубы
            for (float x = bounds.min.x; x < bounds.max.x; x += cubeSize)
            {
                for (float y = bounds.min.y; y < bounds.max.y; y += cubeSize)
                {
                    for (float z = bounds.min.z; z < bounds.max.z; z += cubeSize)
                    {
                        // Создаем новый куб из префаба
                        GameObject cube = Instantiate(_cubePrefab, new Vector3(x, y, z), Quaternion.identity);

                        // Определяем плоскость разреза
                        Vector3 slicePosition = cube.transform.position;
                        Vector3 sliceDirection = Vector3.up;
                        TextureRegion cuttingRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

                        // Разделяем меш объекта с помощью EzySlice
                        GameObject[] slicedObjects = originalMeshGO.SliceInstantiate(slicePosition, sliceDirection, cuttingRegion, originalMaterial);

                        // Удаляем исходный куб
                        Destroy(cube);

                        if (slicedObjects != null)
                        {
                            // Перебираем разделенные объекты и присваиваем им нужные компоненты и материалы
                            foreach (GameObject slicedObject in slicedObjects)
                            {
                                // Добавляем компонент MeshCollider
                                slicedObject.AddComponent<MeshCollider>();

                                var renderer = slicedObject.GetComponent<MeshRenderer>();
                                
                                // Присваиваем им материал объекта
                                renderer.material = meshFilter.GetComponent<Renderer>().material;
                                slicedMeshes.Add(renderer);
                            }
                        }
                    }
                }
            }
        }

        return slicedMeshes;
    }

    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    public void Test()
    {
        var fts = new List<MeshFilter>();
        fts.Add(_test);
        meshRenderers.AddRange(SliceMeshesIntoCubes(fts));
    }
}
