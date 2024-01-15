using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CreateRectangleFromARMesh : MonoBehaviour
{
    public ARMeshManager arMeshManager;
    public GameObject rectanglePrefab; // Префаб прямоугольника

    void Start()
    {
        arMeshManager.meshesChanged += OnMeshesChanged;
    }

    void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshInfo in eventArgs.added)
        {
            // Получаем меш из ARMeshManager
            Mesh mesh = meshInfo.mesh;

            // Создаем новый экземпляр префаба прямоугольника
            GameObject rectangle = Instantiate(rectanglePrefab);

            // Устанавливаем размеры прямоугольника на основе размеров меша
            Vector3 size = new Vector3(mesh.bounds.size.x, 0.01f, mesh.bounds.size.z);
            rectangle.transform.localScale = size;

            // Позиционируем прямоугольник в центре меша
            rectangle.transform.position = mesh.bounds.center;
        }
    }
}