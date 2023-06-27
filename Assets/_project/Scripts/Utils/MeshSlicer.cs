using UnityEngine;
using EzySlice;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Unity.VisualScripting;

public class MeshSlicer : MonoBehaviour
{
    [SerializeField] private float _cubeSize = 10f;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private Material _crossSectionMaterial;
    [SerializeField] private MeshFilter _test;

    public async Task<List<MeshRenderer>> SliceMeshesIntoCubes(MeshFilter meshFilter)
    {
        List<MeshRenderer> slicedMeshes = new List<MeshRenderer>();

        // Создаем список позиций для кубов
        List<Vector3> cubePositions = new List<Vector3>();

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
                    cubePositions.Add(new Vector3(x, y, z));
                }
            }
        }

        // Создаем кубы в главном потоке с использованием async/await
        foreach (Vector3 position in cubePositions)
        {
            GameObject cube = Instantiate(_cubePrefab, position, Quaternion.identity);
            await Task.Yield(); // Даем шанс главному потоку обработать созданный куб

            // Определяем плоскость разреза
            Vector3 slicePosition = cube.transform.position;
            Vector3 horizontalSliceDirection = Vector3.up; // Горизонтальное направление разреза
            Vector3 verticalSliceDirection = Vector3.right; // Вертикальное направление разреза
            TextureRegion cuttingRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

            // Разделяем меш объекта с помощью EzySlice
            GameObject[] horizontalSlicedObjects = originalMeshGO.SliceInstantiate(slicePosition, horizontalSliceDirection, cuttingRegion, originalMaterial);

            if (horizontalSlicedObjects != null)
            {
                foreach (GameObject horizontalSlicedObject in horizontalSlicedObjects)
                {
                    // Разделяем горизонтально разрезанный объект по вертикали
                    GameObject[] slicedObjects = horizontalSlicedObject.SliceInstantiate(slicePosition, verticalSliceDirection, cuttingRegion, originalMaterial);

                    if (slicedObjects != null)
                    {
                        // Перебираем разделенные объекты и присваиваем им нужные компоненты и материалы
                        foreach (GameObject slicedObject in slicedObjects)
                        {
                            // Добавляем компонент MeshCollider
                            slicedObject.AddComponent<MeshCollider>();

                            var renderer = slicedObject.GetComponent<MeshRenderer>();

                            // Присваиваем им материал объекта
                            renderer.material = originalMaterial;
                            slicedObject.transform.SetParent(originalMeshGO.transform.parent, false);
                            slicedMeshes.Add(renderer);
                        }
                    }
                }
            }

            // Удаляем исходный куб
            Destroy(cube);
            
        }
        originalMeshGO.SetActive(false);
        return slicedMeshes;
    }

    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    public async void Test()
    {
        var fts = new List<MeshFilter>();
        fts.Add(_test);

        // Assuming `meshRenderers` is a List<MeshRenderer> variable in your class
        var slicedMeshes = await SliceMeshesIntoCubes(_test);
        meshRenderers.AddRange(slicedMeshes);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_test.mesh.bounds.center, _test.mesh.bounds.size);
    }
}
