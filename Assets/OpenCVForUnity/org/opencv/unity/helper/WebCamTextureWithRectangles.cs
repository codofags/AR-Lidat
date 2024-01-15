using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.Structured_lightModule;
namespace OpenCVForUnityExample
{
public class WebCamTextureWithRectangles : MonoBehaviour
{
    public WebCamTextureToMatHelper webCamTextureToMatHelper;
    public RectanglesWithTexture rectanglesWithTexture; // Прикрепленный компонент

    private void Start()
    {
        webCamTextureToMatHelper.Initialize();

        
    }

    private void OnWebCamTextureToMatHelperInitialized()
    {
        webCamTextureToMatHelper.Play();
    }


private void Update()
{
    if (webCamTextureToMatHelper.IsPlaying())
    {
        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
        rectanglesWithTexture.ProcessFrame(webCamTextureMat);
    }
}

    private void OnDisable()
    {
        webCamTextureToMatHelper.Dispose();
    }
}
}