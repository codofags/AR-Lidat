using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class TestScan : MonoBehaviour
{
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _cameraManager;
    [SerializeField] private Material _nonWireframeMaterial;
    public RawImage rw;
    private Camera _camera;
    private int _textureDevider = 1;
    public Texture2D opencvprocessed;
    [SerializeField] private GameObject plane;
    public Texture2D tex;
    int[,] newFilledPixels;
    int[,] newFilledPixels2;
    [SerializeField] private Texture2D texture2;
    public Vector2 vector2;
    public Texture2D resizedTexture;
    private Point topLeftPoint; // Define these as class fields
    private Point bottomRightPoint;
    public Texture2D[] textures = new Texture2D[50];
    int save;
    public Mat imgMat;
    public Texture2D originalTexture;
    public Texture2D trans;
    Mat changedRegions;
    // Reference to the material whose texture you want to change
    public Material targetMaterial;
    public Texture2D txr;
    public bool isPiked;
    private bool certificateValidationBypassed = false;
    static List<Point> filledPixels = new List<Point>();
    
    bool capitured;
    public double cannyMinThres = 140;
    public Mat mats;
    public Scalar scalrs;
    public double thresholdFactor = 10.0;
    public bool dark;

    private void Start()
    {
        Reporter.Instance.doShow();
        _camera = Camera.main;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;
        CameraPositionSaver.Instance.StartSavingOneFrame();
        ScanStart();
    }

    Texture2D TextureToTexture2D(Texture texture)
    {
        // Создание временного RenderTexture и установка его как активного
        RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        Graphics.Blit(texture, rt);
        RenderTexture.active = rt;

        // Создание нового Texture2D и копирование данных из RenderTexture
        Texture2D texture2D = new Texture2D(texture.width, texture.height);
        texture2D.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
        texture2D.Apply();

        // Освобождение ресурсов
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return texture2D;
    }
    public void ScanStart()
    {
        _arMeshManager.enabled = true; // Включаем ARMeshManager для сканирования мешей
        XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // Получаем доступ к подсистеме ARKitMeshSubsystem

        if (arMeshSubsystem != null)
        {
            arMeshSubsystem.Start();
        }
    }

    private MeshCollider GetMeshOnRaycast(Vector2 touchPosition)
    {
        Ray ray = _camera.ScreenPointToRay(touchPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null)
            {
                return meshCollider;
            }
        }

        return null;
    }
    public void Reset()
    {
        isPiked = false;
    }
    public void Set()
    {
        isPiked = true;
    }
    public void Copy()
    {
        tex = originalTexture;
    }

        
    Point GetPoint()
    {
        Point point = new Point(0, 0);
        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend != null && rend.sharedMaterial != null && rend.sharedMaterial.mainTexture != null && meshCollider != null)
            {
                Texture2D tex = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;
                double adjustedY = tex.height - pixelUV.y;
                point = new Point((double)pixelUV.x, adjustedY);
            }
        }
        return point;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            
            // Choosen color is red by default
            RaycastHit hit;
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                
                {
                    //if (opencvprocessed == null)
                    {
                        if (isPiked == true)
                        {
                            Renderer renderer = hit.collider.GetComponent<Renderer>();

                            Texture texture = renderer.material.mainTexture;
                            Texture2D texture2D = TextureToTexture2D(texture);
                            Debug.Log("Opencv paint");
                            Mat imgMat = new Mat(texture2D.height, texture2D.width, CvType.CV_8UC4);
                            Utils.texture2DToMat(texture2D, imgMat);
                            
                            Paint(imgMat, GetPoint(), new Size(texture2D.width, texture2D.height), new Scalar(255, 0, 0, 255));
                        }
                    }
                }
            }
        }


        if (Input.touchCount > 0) // Проверяем, есть ли касания на экране.
        {
            Touch touch = Input.GetTouch(0); // Берем первое касание (первое пальце).

            if (touch.phase == TouchPhase.Began) // Проверяем, что касание только началось.
            {
                Vector2 touchPosition = touch.position;
                MeshCollider meshCol = GetMeshOnRaycast(touchPosition);

                if (meshCol != null)
                {
                    // Возвращен меш, в которыq попал луч.
                    Debug.Log("Raycast hit a mesh: " + meshCol.name);
                    //Получаем текстуру кадра
                    if (isPiked == false)
                    {
                         tex = CameraPositionSaver.Instance.FrameTexture;
                         
                    }
                    

                        
                    if (tex == null)
                    {
                        Debug.Log("Кадра еще нет!");
                        return;
                    }
                    else
                    {
                        Debug.Log("Кадр есть");
                    }
                    //Тут нужно делать твои действия с OpenCV
                    if (isPiked == true)
                    {
                        Debug.Log("Opencv paint");
                        Mat imgMat = new Mat(tex.height, tex.width, CvType.CV_8UC4);
                        Utils.texture2DToMat(tex, imgMat);

                        Paint(imgMat, GetPoint(), new Size(tex.width, tex.height), new Scalar(255, 0, 0, 255));
                    }
                    // Создаем новый материал.
                    var meshRenderer = meshCol.GetComponent<MeshRenderer>();
                    meshRenderer.material = _nonWireframeMaterial;
                    meshRenderer.material.color = Color.white;
                    var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    Texture2D texture;
                    if (opencvprocessed != null)
                    {
                        Debug.Log("OpenCV texture");
                        texture = opencvprocessed;
                        //newMaterial.SetTexture("_BaseMap", opencvprocessed);
                    }
                    else
                    {
                        Debug.Log("Frame texture");
                        texture = tex;
                    }
                    if (isPiked == false)
                    {
                        meshFilter.GenerateUV(_camera, texture, Vector2.zero);
                        ///isPiked = true;
                    }
                    meshRenderer.material.SetTexture("_BaseMap", texture);

                    // Отключаем ARMeshManager
                    _arMeshManager.enabled = false; 

                    XRMeshSubsystem arMeshSubsystem = _arMeshManager.subsystem;

                    if (arMeshSubsystem != null)
                    {
                        arMeshSubsystem.Stop();
                    }
                }
                else
                {
                    // Луч не попал в меши.
                    Debug.Log("Raycast did not hit mesh.");
                }
            }
        }
    }

    public Texture2D GetAndProcessImageAsync()
    {
        XRCpuImage image;
        if (_cameraManager.TryAcquireLatestCpuImage(out image))
        {
            var request = image.ConvertAsync(new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width / _textureDevider, image.height / _textureDevider),
                outputFormat = TextureFormat.RGB24,
                transformation = XRCpuImage.Transformation.MirrorY
            });

            while (!request.status.IsDone())
            {

            }

            if (request.status == XRCpuImage.AsyncConversionStatus.Ready)
            {
                var rawData = request.GetData<byte>();
                var texture = new Texture2D(
                    request.conversionParams.outputDimensions.x,
                    request.conversionParams.outputDimensions.y,
                    request.conversionParams.outputFormat,
                    false);

                texture.LoadRawTextureData(rawData);
                texture.Apply();

                texture = texture.RotateTexture(false);

                request.Dispose();
                image.Dispose();

                return texture;
            }

            request.Dispose();
            image.Dispose();
        }

        return null;
    }



    public void GenerateSimpleUV(Texture2D texture, ref MeshFilter meshFilter)
    {
        Debug.Log("Generate Simple UV");
        Mesh mesh = new Mesh();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3[] normals = meshFilter.mesh.normals;
        int[] faces = meshFilter.mesh.triangles;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = faces;

        Vector2[] textureCoordinates = CalcSimpleTextureCoordinates(mesh, texture);
        mesh.uv = textureCoordinates;
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }
    Mat Paint(Mat image, Point p, Size imageSize, Scalar chosenColor)
    {
        double ratio = 2.5;
        Mat mRgbMat = image;
        Mat sharpenedMat = new Mat();
        Imgproc.cvtColor(mRgbMat, mRgbMat, Imgproc.COLOR_RGBA2RGB);
        //Imgproc.Laplacian(mRgbMat, sharpenedMat, CvType.CV_8U);

        // Исходное изображение + коэффициент * (изображение после фильтрации - исходное изображение)
        //double alpha = 0.5;  // Подстройте коэффициент alpha по вашему усмотрению
        // Core.addWeighted(mRgbMat, 1 + alpha, sharpenedMat, -alpha, 0, mRgbMat);
        Mat mask = new Mat(new Size(mRgbMat.cols() / 8.0f, mRgbMat.rows() / 8.0f), CvType.CV_8UC1, new Scalar(0.0));
        Mat img = new Mat();
        mRgbMat.copyTo(img);
        //grayscale
        Mat mGreyScaleMat = new Mat();
        Imgproc.cvtColor(mRgbMat, mGreyScaleMat, Imgproc.COLOR_RGB2GRAY, 3);
        //Imgproc.equalizeHist(mGreyScaleMat, mGreyScaleMat);

        Core.MinMaxLocResult minMaxResult = Core.minMaxLoc(mGreyScaleMat);

        // Calculate the scaling factor to equalize the brightness
        double scale = 255.0 / (minMaxResult.maxVal - minMaxResult.minVal);

        // Apply the scaling factor to equalize brightness
        mGreyScaleMat.convertTo(mGreyScaleMat, CvType.CV_8UC1, scale, -minMaxResult.minVal * scale);
        Mat cannyGreyMat = new Mat();

        double meanBrightness = 0.0;
        int totalPixels = 0;

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                double[] pixel = mGreyScaleMat.get(y, x);
                meanBrightness += pixel[0];
                totalPixels++;
            }
        }

        meanBrightness /= totalPixels;
        // Измените это значение по своему усмотрению
        Debug.Log(dark);
        if (dark == false)
        {
            cannyMinThres = 40;
        }

        else
        {
            cannyMinThres = 100;
        }
        Imgproc.Canny(mGreyScaleMat, cannyGreyMat, cannyMinThres, cannyMinThres * ratio, 3);
        Core.normalize(cannyGreyMat, cannyGreyMat, 0, 255, Core.NORM_MINMAX);
        // Imgproc.equalizeHist(cannyGreyMat, cannyGreyMat);
        //ShowImage(cannyGreyMat);
        //hsv
        Mat hsvImage = new Mat();
        Imgproc.cvtColor(img, hsvImage, Imgproc.COLOR_RGB2HSV);
        //got the hsv values
        List<Mat> list = new List<Mat>(3);
        Core.split(hsvImage, list);
        Mat sChannelMat = new Mat();
        List<Mat> slist = new List<Mat>();
        slist.Add(list[1]);
        Core.merge(slist, sChannelMat);
        ///Imgproc.medianBlur(mRgbMat, mRgbMat, 7);
        // canny
        Mat cannyMat = new Mat();
        Imgproc.Canny(sChannelMat, cannyMat, cannyMinThres, cannyMinThres * ratio, 3);
        //ShowImage(cannyMat);
        Core.addWeighted(cannyMat, 0.5, cannyGreyMat, 0.5, 0.0, cannyMat);
        Imgproc.dilate(cannyMat, cannyMat, mask, new Point(0.0, 0.0), 5);
        //ShowImage(cannyMat);
        double width = imageSize.width;
        double height = imageSize.height;
        Point seedPoint = p;//new Point(p.x * (mRgbMat.width() / width), p.y * (mRgbMat.height() / height));
        // Make sure to resize the cannyMat or it'll throw an error
        Imgproc.resize(cannyMat, cannyMat, new Size(cannyMat.width() + 2.0, cannyMat.height() + 2.0));
        //Imgproc.medianBlur(mRgbMat, mRgbMat, 7);
        //ShowImage(mRgbMat);
        int floodFillFlag = 8;
        Imgproc.floodFill(
            mRgbMat,
            cannyMat,
            seedPoint,
            chosenColor,
            new OpenCVForUnity.CoreModule.Rect(0, 0, 0, 0),
            new Scalar(5.0, 5.0, 5.0),
            new Scalar(5.0, 5.0, 5.0),
            floodFillFlag
        );
        Imgproc.dilate(mRgbMat, mRgbMat, mask, new Point(0.0, 0.0), 5);
        //got the hsv of the mask image
        Mat rgbHsvImage = new Mat();
        Imgproc.cvtColor(mRgbMat, rgbHsvImage, Imgproc.COLOR_RGB2HSV);
        List<Mat> list1 = new List<Mat>(3);
        Core.split(rgbHsvImage, list1);
        //merged the �v� of original image with mRgb mat
        Mat result = new Mat();
        List<Mat> newList = new List<Mat>();
        newList.Add(list1[0]);
        newList.Add(list1[1]);
        newList.Add(list1[2]);
        Core.merge(newList, result);
        // converted to rgb
        Imgproc.cvtColor(result, result, Imgproc.COLOR_HSV2RGB);

        Core.addWeighted(result, 0.7, img, 0.3, 0.0, result);

        result = ProcessResult(result, chosenColor, 110, texture2);
        mats = result;
        scalrs = chosenColor;
        return result;
    }

    Mat ProcessResult(Mat result, Scalar colorToRemove, int tolerance, Texture2D texture)
    {
        int rows = result.rows();
        int cols = result.cols();
        byte[] rgbData = new byte[3];
        HashSet<Vector2Int> processedPixels = new HashSet<Vector2Int>();
        Mat processedResult = new Mat(rows, cols, CvType.CV_8UC4);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector2Int pixelPosition = new Vector2Int(x, y);

                // Проверяем, был ли этот пиксель уже обработан
                

                result.get(y, x, rgbData);

                bool colorMatch = true;
                for (int i = 0; i < 3; i++)
                {
                    if (Math.Abs(rgbData[i] - colorToRemove.val[i]) > tolerance)
                    {
                        colorMatch = false;
                        break;
                    }
                }

                if (colorMatch)
                {
                    Color textureColor = texture.GetPixel(x, y);

                    byte redByte = (byte)(textureColor.r * 255);
                    byte greenByte = (byte)(textureColor.g * 255);
                    byte blueByte = (byte)(textureColor.b * 255);

                    rgbData[0] = redByte;
                    rgbData[1] = greenByte;
                    rgbData[2] = blueByte;

                    // Отмечаем пиксель как обработанный
                    processedPixels.Add(pixelPosition);
                }

                byte[] rgbaData = { rgbData[0], rgbData[1], rgbData[2], (byte)255 };
                processedResult.put(y, x, rgbaData);
            }

        }
        ShowImage(processedResult);
        return processedResult;
    }
    void ShowImage(Mat mat)
    {
        opencvprocessed = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, opencvprocessed);
        Debug.Log("PAINT");
        rw.texture = opencvprocessed;

    }

    private static Vector2[] CalcSimpleTextureCoordinates(Mesh geometry, Texture2D texture)
    {
        Vector2[] textureCoordinates = new Vector2[geometry.vertices.Length];

        if (texture != null && texture.width > 0 && texture.height > 0)
        {
            for (int i = 0; i < geometry.vertices.Length; i++)
            {
                // Просто нормализуем координаты текстуры на основе размеров текстуры.
                textureCoordinates[i] = new Vector2(geometry.vertices[i].x / texture.width, geometry.vertices[i].z / texture.height);
            }
        }
        else
        {
            // Если текстура отсутствует или ее размеры некорректны, устанавливаем стандартные UV-координаты.
            for (int i = 0; i < geometry.vertices.Length; i++)
            {
                textureCoordinates[i] = Vector2.zero;
            }
        }

        return textureCoordinates;
    }
}
