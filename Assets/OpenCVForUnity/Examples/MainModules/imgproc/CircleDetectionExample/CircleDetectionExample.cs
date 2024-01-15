#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

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
    /// <summary>
    /// Circle Detection Example
    /// An example of circle detection using the Imgproc.HoughCircles function.
    /// http://docs.opencv.org/3.1.0/d4/d70/tutorial_hough_circle.html
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class CircleDetectionExample : MonoBehaviour
    {
        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;
    public int planeDetectionIterations = 100;
    public double planeDetectionDistanceThreshold = 0.01;
    Dictionary<int, Point> previousCircleCenters ;
    Dictionary<int, double> previousDistances;
    public Text circle;
    private MatOfPoint3f planePoints;
    
    int furtherCirclesCount;
    
    int circlesCount;
    int closerCirclesCount;
    private MatOfPoint3f bestPlanePoints;
    private MatOfPoint2f bestPlanePoints2D;
    private MatOfPoint2f imagePoints;
    private MatOfDouble cameraMatrix;
    private MatOfDouble distortionCoefficients;

    private bool planeDetected = false;
    bool wasCloser;

    // other fields
   
   
   

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        // Use this for initialization
        void Start ()
        {
            fpsMonitor = GetComponent<FpsMonitor> ();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
            #endif
            webCamTextureToMatHelper.Initialize ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(webCamTextureMat, texture, webCamTextureToMatHelper.GetBufferColors());

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);

            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null) {
                fpsMonitor.Add ("width", webCamTextureMat.width ().ToString ());
                fpsMonitor.Add ("height", webCamTextureMat.height ().ToString ());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString ());
            }

                                    
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
                                    
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");
            if (grayMat != null)
                grayMat.Dispose ();

            if (texture != null) {
                Texture2D.Destroy (texture);
                texture = null;
            }                        
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
void Update()
{
    if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
    {
        Mat rgbaMat = webCamTextureToMatHelper.GetMat();
        Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

        if (previousCircleCenters == null)
        {
            previousCircleCenters = new Dictionary<int, Point>();
            previousDistances = new Dictionary<int, double>();
        }

        using (Mat circles = new Mat())
        {
            Imgproc.HoughCircles(grayMat, circles, Imgproc.CV_HOUGH_GRADIENT, 2, 10, 160, 50, 90, 100);
            Point pt = new Point();

            for (int i = 0; i < circles.cols(); i++)
            {
                double[] data = circles.get(0, i);
                pt.x = data[0];
                pt.y = data[1];
                double rho = data[2];

                bool wasDetected = previousCircleCenters.ContainsKey(i);

                if (wasDetected)
                {
                    Point previousCenter = previousCircleCenters[i];
                    double previousDistance = previousDistances[i];

                    double distance = Math.Sqrt(Math.Pow(pt.x - previousCenter.x, 2) + Math.Pow(pt.y - previousCenter.y, 2));

                    if (previousDistance < distance)
                    {
                        if (wasCloser)
                        {
                            // circle got farther after being closer
                            circlesCount++;
                            wasCloser = false;
                        }
                    }
                    else if (previousDistance > distance)
                    {
                        if (!wasCloser)
                        {
                            // circle got closer after being farther
                            circlesCount++;
                            wasCloser = true;
                        }
                    }
                }

                previousCircleCenters[i] = pt;
                previousDistances[i] = Math.Sqrt(Math.Pow(pt.x - Screen.width / 2, 2) + Math.Pow(pt.y - Screen.height / 2, 2));
                circle.text=circlesCount.ToString();
                Imgproc.circle(rgbaMat, pt, (int)rho, new Scalar(255, 0, 0, 255), 5);
            }
        }

        Utils.matToTexture2D(rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors());
    }
}
//DetectPlane();
//                Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

    
  
public void DetectPlane()
{
    // Convert the current camera frame to a grayscale image
    Mat rgbaMat = webCamTextureToMatHelper.GetMat();
    Mat grayMat = new Mat();
    Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

    // Detect edges in the grayscale image
    Mat cannyMat = new Mat();
    Imgproc.Canny(grayMat, cannyMat, 50, 200);
  
    // Find contours in the binary image
    List<MatOfPoint> contours = new List<MatOfPoint>();
    Mat hierarchy = new Mat();
    Imgproc.findContours(cannyMat, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

    // Find the largest contour
    double maxArea = -1;
    MatOfPoint largestContour = null;
    for (int i = 0; i < contours.Count; i++)
    {
        double area = Imgproc.contourArea(contours[i]);
        if (area > maxArea)
        {
            maxArea = area;
            largestContour = contours[i];
        }
    }

    // Fit a plane to the largest contour using least-squares regression
    MatOfPoint3f objectPoints = new MatOfPoint3f();
    MatOfPoint2f imagePoints = new MatOfPoint2f();
    for (int i = 0; i < largestContour.rows(); i++)
    {
        Point contourPoint = largestContour.toList()[i];
        objectPoints.push_back(new MatOfPoint3f(new Point3(contourPoint.x, contourPoint.y, 0)));
        imagePoints.push_back(new MatOfPoint2f(contourPoint));
    }
    Mat rotationVector = new Mat();
    Mat translationVector = new Mat();
    Calib3d.solvePnP(objectPoints, imagePoints, cameraMatrix, distortionCoefficients, rotationVector, translationVector);
    MatOfPoint3f planePoints = new MatOfPoint3f(
        new Point3(-1, -1, 0),
        new Point3(-1, 1, 0),
        new Point3(1, 1, 0),
        new Point3(1, -1, 0)
    );
    MatOfPoint2f projectedPoints = new MatOfPoint2f();
    Calib3d.projectPoints(planePoints, rotationVector, translationVector, cameraMatrix, distortionCoefficients, projectedPoints);

    // Draw the plane on the camera frame
    List<Point> projectedPointsList = projectedPoints.toList();
    Imgproc.line(rgbaMat, projectedPointsList[0], projectedPointsList[1], new Scalar(0, 255, 0), 5);
    Imgproc.line(rgbaMat, projectedPointsList[1], projectedPointsList[2], new Scalar(0, 255, 0), 5);
    Imgproc.line(rgbaMat, projectedPointsList[2], projectedPointsList[3], new Scalar(0, 255, 0), 5);
    Imgproc.line(rgbaMat, projectedPointsList[3], projectedPointsList[0], new Scalar(0, 255, 0), 5);
    using (Mat circles = new Mat())
{
    Imgproc.HoughCircles(grayMat, circles, Imgproc.CV_HOUGH_GRADIENT, 2, 10, 160, 50, 90, 100);
    Point pt = new Point();
     MatOfPoint2f projectedPolygon = new MatOfPoint2f();

    int numCirclesOnPlane = 0; // Initialize a counter for circles on the plane

    for (int i = 0; i < circles.cols(); i++)
    {
        double[] data = circles.get(0, i);
        pt.x = data[0];
        pt.y = data[1];
        double rho = data[2];

        // Check if the center of the circle is within the projected polygon
        if (Imgproc.pointPolygonTest(new MatOfPoint2f(projectedPolygon.toArray()), pt, false) > 0)
        {
            numCirclesOnPlane++;
        }

        Imgproc.circle(rgbaMat, pt, (int)rho, new Scalar(255, 0, 0, 255), 5);
    }
circlesCount = numCirclesOnPlane; // Update the circles count based on the number of circles on the plane
    
}

   
//Utils.matToTexture2D(rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors());
}
        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            webCamTextureToMatHelper.Dispose ();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("OpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.requestedIsFrontFacing;
        }
    }
}

#endif