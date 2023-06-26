using UnityEngine;

public class MeshSlicer : MonoBehaviour
{
    public GameObject cubePrefab; // Префаб куба

    void Start()
    {
        // Получение меша объекта
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            return;
        }

        Mesh mesh = meshFilter.mesh;

        // Получение границ объекта
        Bounds bounds = mesh.bounds;

        // Размеры куба
        float cubeSize = 0.1f; // 10 см (0.1 м)

        // Количество кубов вдоль каждой оси
        int numCubesX = Mathf.CeilToInt(bounds.size.x / cubeSize);
        int numCubesY = Mathf.CeilToInt(bounds.size.y / cubeSize);
        int numCubesZ = Mathf.CeilToInt(bounds.size.z / cubeSize);

        // Создание кубов
        for (int x = 0; x < numCubesX; x++)
        {
            for (int y = 0; y < numCubesY; y++)
            {
                for (int z = 0; z < numCubesZ; z++)
                {
                    // Рассчитываем позицию каждого куба
                    Vector3 cubePosition = bounds.min + new Vector3(x * cubeSize, y * cubeSize, z * cubeSize) + new Vector3(cubeSize / 2f, cubeSize / 2f, cubeSize / 2f);

                    // Создание куба
                    GameObject cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);

                    // Установка размера куба
                    cube.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

                    // Разделение меша с использованием куба
                    SlicedHull slicedHull = MeshCut.Slice(mesh, cube.transform.position, cube.transform.up);

                    // Проверка успешного разделения меша
                    if (slicedHull != null)
                    {
                        // Создание новых игровых объектов с разрезанными мешами
                        CreateSlicedObject(slicedHull.GetUpperHull(), cube.transform.position);
                        CreateSlicedObject(slicedHull.GetLowerHull(), cube.transform.position);
                    }
                }
            }
        }

        // Удаление исходного объекта
        Destroy(gameObject);
    }

    void CreateSlicedObject(Mesh slicedMesh, Vector3 position)
    {
        // Создание нового игрового объекта и присвоение разрезанного меша
        GameObject slicedObject = new GameObject("Sliced Object");
        slicedObject.AddComponent<MeshFilter>().mesh = slicedMesh;
        slicedObject.AddComponent<MeshRenderer>();
        slicedObject.transform.position = position;
    }
}
