using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CameraTextureCapture : MonoBehaviour
{
    [SerializeField] private ARCameraManager _arCameraManager;

    private void Start()
    {
        // Проверка наличия компонента ARCameraManager
        if (_arCameraManager == null)
        {
            _arCameraManager = FindObjectOfType<ARCameraManager>();
        }
    }

    private void Update()
    {
        // Проверка, что AR камера готова и текстура доступна
        if (_arCameraManager != null && _arCameraManager.TryAcquireLatestCpuImage(out var image))
        {
            // Создание текстуры из AR камеры
            Texture2D cameraTexture = CreateTextureFromCameraImage(image);

            // Освобождение ресурсов AR камеры
            image.Dispose();
        }
    }

    private Texture2D CreateTextureFromCameraImage(XRCpuImage image)
    {
        // Получение данных из XRCpuImage
        XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams(image, TextureFormat.RGBA32);
        NativeArray<byte> rawTextureData = image.GetPlane(0).data;
        int width = image.width;
        int height = image.height;

        // Создание новой текстуры
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Заполнение текстуры полученными данными
        texture.LoadRawTextureData(rawTextureData);
        texture.Apply();

        // Возврат созданной текстуры
        return texture;
    }
    public Texture2D GetCameraTexture()
    {
        if (_arCameraManager != null && _arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Texture2D cameraTexture = CreateTextureFromCameraImage(image);
            image.Dispose();
            return cameraTexture;
        }
        return null;
    }
}
