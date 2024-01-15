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
public class RectanglesWithTexture : MonoBehaviour
{


     public Texture2D fillTexture;
     private Texture2D outputTexture;

    public void ProcessFrame(Mat inputMat)
    {
         Debug.Log("-");
        // Конвертация в градации серого для облегчения работы с изображением
        Mat grayMat = new Mat();
        Imgproc.cvtColor(inputMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

        // Применение алгоритма поиска контуров
        Mat hierarchy = new Mat();
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Imgproc.findContours(grayMat, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

        // Поиск и обработка прямоугольных контуров
        foreach (var contour in contours)
        {
            MatOfPoint2f approxCurve = new MatOfPoint2f();
            MatOfPoint2f contour2f = new MatOfPoint2f(contour.toArray());
          
            double epsilon = 0.04 * Imgproc.arcLength(contour2f, true);
            Imgproc.approxPolyDP(contour2f, approxCurve, epsilon, true);
 Debug.Log(approxCurve.total());
            if (approxCurve.total() == 4)
            {
                 
                // Заполнение найденного прямоугольника текстурой
                Point[] points = approxCurve.toArray();
                for (int i = 0; i < 4; i++)
{
    Point pt1 = new Point(points[i].x, inputMat.height() - points[i].y);
    Point pt2 = new Point(points[(i + 1) % 4].x, inputMat.height() - points[(i + 1) % 4].y);
    Imgproc.line(inputMat, pt1, pt2, new Scalar(255, 0, 0, 255), 2); // Красный цвет в формате BGR
}

                OpenCVForUnity.CoreModule.Rect boundingRect = Imgproc.boundingRect(new MatOfPoint(approxCurve.toArray()));
               Mat textureROI = new Mat(inputMat, boundingRect);
Mat fillTextureMat = new Mat(fillTexture.height, fillTexture.width, CvType.CV_8UC4); // Используйте CV_8UC4 для текстур с альфа-каналом
Utils.texture2DToMat(fillTexture, fillTextureMat);
fillTextureMat.copyTo(textureROI);
                 outputTexture = new Texture2D(inputMat.cols(), inputMat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(inputMat, outputTexture);
        GetComponent<Renderer>().material.mainTexture = outputTexture;
            }
        }
    }
}