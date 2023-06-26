using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public static class ARCameraExtension
{

    public static Pose GetCameraPose(this ARCameraManager _arCameraManager)
    {
        if (_arCameraManager != null)
        {
            // Получаем позицию и поворот камеры
            Vector3 cameraPosition = _arCameraManager.transform.position;
            Quaternion cameraRotation = _arCameraManager.transform.rotation;

            Pose pose = new Pose(cameraPosition, cameraRotation);
            return pose;
        }
        // Возвращаем пустую позицию и поворот, если данные камеры недоступны
        return new Pose();
    }


    public static Texture2D GetCameraTexture(this ARCameraManager _arCameraManager)
    {
        if (_arCameraManager != null && _arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Texture2D cameraTexture = CreateTextureFromCameraImage(image);
            image.Dispose();
            return cameraTexture;
        }
        return null;
    }

    private static Texture2D CreateTextureFromCameraImage(XRCpuImage image)
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
}