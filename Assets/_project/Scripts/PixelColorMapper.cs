//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;

//public class PixelColorMapper : MonoBehaviour
//{
//    [SerializeField] private ARMeshManager arMeshManager;
//    [SerializeField] private MeshFilter meshFilter;

//    private Texture2D currentTexture;

//    void Start()
//    {
//        arMeshManager.meshesChanged += OnMeshesChanged;
//    }

//    void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
//    {
//        foreach (var meshFilter in eventArgs.added)
//        {
//            // Получаем границы меша
//            var bounds = meshFilter.mesh.bounds;

//            // Получаем текущий кадр с камеры
//            var currentTexture = ARCameraManager.currentTexture;

//            // Получаем массив цветов пикселей
//            var pixels = currentTexture.GetPixels();

//            // Получаем массив вершин и треугольников меша
//            var vertices = meshFilter.mesh.vertices;
//            var triangles = meshFilter.mesh.triangles;

//            // Создаем новый массив цветов вершин
//            var colors = new Color[vertices.Length];

//            // Проходим по каждой вершине меша
//            for (int i = 0; i < vertices.Length; i++)
//            {
//                var vertex = vertices[i];

//                // Проверяем, находится ли вершина в области сканирования
//                if (vertex.x >= bounds.min.x && vertex.x <= bounds.max.x &&
//                    vertex.y >= bounds.min.y && vertex.y <= bounds.max.y &&
//                    vertex.z >= bounds.min.z && vertex.z <= bounds.max.z)
//                {
//                    // Вычисляем координаты пикселя
//                    var u = (vertex.x - bounds.min.x) / (bounds.max.x - bounds.min.x);
//                    var v = (vertex.y - bounds.min.y) / (bounds.max.y - bounds.min.y);

//                    // Получаем соответствующий пиксель из массива цветов пикселей
//                    var pixelX = Mathf.FloorToInt(u * currentTexture.width);
//                    var pixelY = Mathf.FloorToInt(v * currentTexture.height);
//                    var pixelIndex = pixelY * currentTexture.width + pixelX;

//                    // Назначаем цвет пикселя вершине меша
//                    colors[i] = pixels[pixelIndex];
//                }
//            }

//            // Присваиваем массив цветов вершин меша
//            meshFilter.mesh.colors = colors;
//        }
//    }
//}
