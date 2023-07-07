using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TextureGetter : Singleton<TextureGetter>
{
    [SerializeField] private RawImage _showTextureImage;
    [SerializeField] private ARCameraManager _cameraManager;

    private Texture2D _destinationTexture;
    private bool _isPerformingScreenGrab = false;
    private bool _isRotateEnabled = false;
    private Action<Texture2D, int> _onTextureGetted;
    public void Initialize(Action<Texture2D, int> onTextureGetted)
    {
        _onTextureGetted = onTextureGetted;
    }

    private void Start()
    {
        //Camera.onPostRender += OnPostRenderCallback;
    }

    public void EnableRotate()
    {
        _isRotateEnabled = true;
    }

    public void GetImageFromRnderTexture()
    {
        _destinationTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        _showTextureImage.texture = _destinationTexture;

        _isPerformingScreenGrab = true;
    }

    public void GetImageAsync(int id)
    {
        // Get information about the device camera image.
        if (_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.Log(Camera.main.transform.rotation.eulerAngles);
            // If successful, launch a coroutine that waits for the image
            // to be ready, then apply it to a texture.
            StartCoroutine(ProcessImage(image, id));

            // It's safe to dispose the image before the async operation completes.
            image.Dispose();
        }
    }

    IEnumerator ProcessImage(XRCpuImage image, int id)
    {
        //Debug.Log($"Image: {image.width}/{image.height}. Camera: {Camera.main.pixelWidth}/{Camera.main.pixelHeight}");

        // Create the async conversion request.
        var request = image.ConvertAsync(new XRCpuImage.ConversionParams
        {
            // Use the full image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width, image.height),

            // Color image format.
            outputFormat = TextureFormat.RGB24,

            // Flip across the Y axis.
            transformation = XRCpuImage.Transformation.MirrorY
        });

        // Wait for the conversion to complete.
        while (!request.status.IsDone())
            yield return null;

        // Check status to see if the conversion completed successfully.
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            // Something went wrong.
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            // Dispose even if there is an error.
            request.Dispose();
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();
        //Debug.Log($"Texture size: {rawData.Length / 1024} kb");

        var texture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false);

        // Copy the image data into the texture.
        texture.LoadRawTextureData(rawData);
        texture.Apply();


        texture = texture.RotateTexture(false);


        _showTextureImage.texture = texture;
        // Need to dispose the request to delete resources associated
        // with the request, including the raw data.
        request.Dispose();

        _onTextureGetted?.Invoke(texture, id);
    }


    void OnPostRenderCallback(Camera cam)
    {
        if (_isPerformingScreenGrab)
        {
            // Check whether the Camera that just finished rendering is the one you want to take a screen grab from
            if (cam.tag == "TextureCamera")
            {
                // Define the parameters for the ReadPixels operation
                Rect regionToReadFrom = new Rect(0, 0, Screen.width, Screen.height);
                int xPosToWriteTo = 0;
                int yPosToWriteTo = 0;
                bool updateMipMapsAutomatically = false;

                // Copy the pixels from the Camera's render target to the texture
                _destinationTexture.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo, updateMipMapsAutomatically);

                // Upload texture data to the GPU, so the GPU renders the updated texture
                // Note: This method is costly, and you should call it only when you need to
                // If you do not intend to render the updated texture, there is no need to call this method at this point
                _destinationTexture.Apply();

                // Reset the isPerformingScreenGrab state
                _isPerformingScreenGrab = false;
            }
        }
    }
}
