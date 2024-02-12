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




public class TestScan : MonoBehaviour
{
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _cameraManager;
    public GameObject MeshObject;
    [SerializeField] private Material _nonWireframeMaterial;
    private Scalar circleColor = new Scalar(0, 0, 0); // Цвет окружности (BGR формат)
    public Texture2D camtexute;
    Renderer rend;
    private Camera _camera;
    private int _textureDevider = 1;
    public Texture2D opencvprocessed;
    [SerializeField] private GameObject plane;
    private MeshFilter[] meshFilters;
    int[,] newFilledPixels;
    int[,] newFilledPixels2;
    public Texture2D texture2;
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
    public UnityEngine.UI.Slider sl;
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

     
        _camera = Camera.main;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;
        newFilledPixels = new int[2048, 2048];
        newFilledPixels2 = new int[2048, 2048];
        meshFilters = new MeshFilter[0];
        CameraPositionSaver.Instance.StartSavingOneFrame();
        ScanStart();
        
    }
public void GetTexture(string textureURL)
    {
       LoadTextureFromResources(textureURL);
    }
    public void LoadTextureFromResources(string textureName)
    {
        // Загрузка текстуры из ресурсов по имени, заданному в параметре textureName
        Texture2D loadedTexture = Resources.Load<Texture2D>(textureName);

        // Проверка на успешную загрузку текстуры
        if (loadedTexture != null)
        {
            // Назначить загруженную текстуру на компонент Renderer
            texture2 = loadedTexture;
        }
        else
        {
           Debug.Log("ОШИБКА НЕТ ТЕКТУРЫ");
        }
    }

   public void Combine()
    {
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            // Проверяем, есть ли у объекта компонент MeshFilter
            MeshFilter meshFilterl = meshRenderer.gameObject.GetComponent<MeshFilter>();

            // Если MeshFilter отсутствует, добавляем его
            if (meshFilterl == null)
            {
                meshFilterl = meshRenderer.gameObject.AddComponent<MeshFilter>();
            }

            // Присваиваем Mesh из MeshRenderer в MeshFilter
            meshFilterl.mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;


            // Добавляем MeshFilter в массив, если его там еще нет
            if (System.Array.IndexOf(meshFilters, meshFilterl) == -1)
            {
                // Добавляем MeshFilter в массив
                System.Array.Resize(ref meshFilters, meshFilters.Length + 1);
                meshFilters[meshFilters.Length - 1] = meshFilterl;
            }
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        MeshFilter meshFilter = MeshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine, true);
        MeshObject.AddComponent<MeshRenderer>().material = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
        var tex = CameraPositionSaver.Instance.FrameTexture;
        if (tex != null)
        {
            meshFilter.GenerateUV(_camera, tex, Vector2.zero);
            originalTexture = tex;
            MeshObject.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", tex);

        }
        else
        {
            meshFilter.GenerateUV(_camera, camtexute, Vector2.zero);
            MeshObject.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", camtexute);
        }

        _arMeshManager.enabled = false;
        MeshCollider meshCollider = MeshObject.GetComponent<MeshRenderer>().gameObject.GetComponent<MeshCollider>();

        // Если MeshCollider отсутствует, добавляем его
        if (meshCollider == null)
        {
            meshCollider = MeshObject.GetComponent<MeshRenderer>().gameObject.AddComponent<MeshCollider>();
        }

        // Присваиваем Mesh из связанного MeshFilter в MeshCollider
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        XRMeshSubsystem arMeshSubsystem = _arMeshManager.subsystem;

        if (arMeshSubsystem != null)
        {
            arMeshSubsystem.Stop();
        }
        isPiked = true;
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
    
    public void Sld()
    {
        cannyMinThres = sl.value;
    }

        
        
    Point GetPoint()
    {
        Point point = new Point(0, 0);
        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            
            //if (rend != null && rend.sharedMaterial != null && rend.sharedMaterial.mainTexture != null && meshCollider != null)
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
                            rend = renderer;
                            Texture texture = renderer.material.mainTexture;
                            Texture2D texture2D = TextureToTexture2D(texture);
                            Debug.Log("Opencv paint");
                            Mat imgMat = new Mat(texture2D.height, texture2D.width, CvType.CV_8UC4);
                            Utils.texture2DToMat(texture2D, imgMat);
                            ///txt.text = "PLENAKA SET";
                            Paint(imgMat, GetPoint(), new Size(texture2D.width, texture2D.height), new Scalar(0,255, 0, 255));
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
                    ///if (isPiked == false)
                    {
                        var tex = CameraPositionSaver.Instance.FrameTexture;
                    }





                        
                        //Тут нужно делать твои действия с OpenCV
                        //if (isPiked == true)
                        {
                            Debug.Log("Opencv paint");
                            Mat imgMat = new Mat(originalTexture.height, originalTexture.width, CvType.CV_8UC4);
                            Utils.texture2DToMat(originalTexture, imgMat);

                            Paint(imgMat, GetPoint(), new Size(originalTexture.width, originalTexture.height), new Scalar(0, 255, 0, 255));

                        }
                        /*
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
                      
                        {
                      
                            ///isPiked = true;
                        }
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh != null && mesh.uv != null && mesh.uv.Length > 0)
                    {
                        meshRenderer.material.SetTexture("_BaseMap", texture);
                    }
                    else
                    {
                        Debug.Log("NO UV");
                    }
                    */
                        // Отключаем ARMeshManager
                        
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
        Imgproc.cvtColor(mRgbMat, mRgbMat, Imgproc.COLOR_RGBA2RGB);
        Mat mask = new Mat(new Size(mRgbMat.cols() / 8.0f, mRgbMat.rows() / 8.0f), CvType.CV_8UC1, new Scalar(0.0));
        Mat img = new Mat();
        mRgbMat.copyTo(img);
        //grayscale
        Mat mGreyScaleMat = new Mat();
        Imgproc.cvtColor(mRgbMat, mGreyScaleMat, Imgproc.COLOR_RGB2GRAY, 3);

        Mat cannyGreyMat = new Mat();
        Imgproc.Canny(mGreyScaleMat, cannyGreyMat, cannyMinThres, cannyMinThres * ratio, 3);

        Mat hsvImage = new Mat();
        Imgproc.cvtColor(img, hsvImage, Imgproc.COLOR_RGB2HSV);
        //got the hsv values
        List<Mat> list = new List<Mat>(3);
        Core.split(hsvImage, list);
        Mat sChannelMat = new Mat();
        List<Mat> slist = new List<Mat>();
        slist.Add(list[1]);
        Core.merge(slist, sChannelMat);
        Imgproc.medianBlur(mRgbMat, mRgbMat, 7);
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
        //merged the “v” of original image with mRgb mat
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
        ///ShowImage(result);
        return result; ;
    }

    Mat ProcessResult(Mat result, Scalar colorToRemove, int tolerance, Texture2D texture)
    {
        int rows = result.rows();
        int cols = result.cols();

        Debug.Log(result.rows());
        Debug.Log(result.cols());
        byte[] rgbData = new byte[3];
        byte[] rgbData2 = new byte[4];
        int[,] filledPixels = new int[rows, cols];

        // Создайте новый Mat для сохранения только соответствующих пикселей
        Mat processedResult = new Mat(originalTexture.height, originalTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(originalTexture, processedResult);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                result.get(y, x, rgbData);
                processedResult.get(y, x, rgbData2);
                bool colorMatch = true;
                for (int i = 0; i < 3; i++)
                {
                    if (Math.Abs(rgbData[i] - colorToRemove.val[i]) > 100)
                    {
                        colorMatch = false;
                        rgbData[0] = rgbData2[0];
                        rgbData[1] = rgbData2[1];
                        rgbData[2] = rgbData2[2];
                        break;
                    }
                }

                if (colorMatch)
                {
                    Color textureColor = texture.GetPixel(x, y);

                    {
                        filledPixels[y, x] = 1;

                    }

                }

                else
                {

                    filledPixels[y, x] = 0;
                }

            }
        }


        for (int x = 0; x < cols; x++)
        {
            int firstFilledY = -1; // Индекс первой заполненной строки
            int lastFilledY = -1;  // Индекс последней заполненной строки
            int filledCount = 0;   // Количество заполненных пикселей в столбце

            for (int y = 0; y < rows; y++)
            {
                if (filledPixels[y, x] == 1)
                {
                    if (firstFilledY == -1)
                    {
                        firstFilledY = y;
                    }
                    lastFilledY = y;
                    filledCount++;
                    //Debug.Log(filledCount);
                }
            }

            if (filledCount >= 0)  // Замените yourThresholdValue на нужное вам значение
            {
                for (int y = firstFilledY + 1; y < lastFilledY; y++)
                {
                    if (newFilledPixels2[y, x] == 0)
                    {
                        if (filledPixels[y, x] == 0)
                        {
                            filledPixels[y, x] = 1;
                            newFilledPixels2[y, x] = 1;
                            // Примените fillTexture к текущему пикселю

                        }
                    }
                }
            }
            firstFilledY = -1;
            lastFilledY = -1;
            filledCount = 0;
        }
        for (int y = 0; y < rows; y++)
        {
            int countFilledPixelsInRow = 0;
            int startX = -1; // Индекс начала залитой области
            int endX = -1;   // Индекс конца залитой области

            for (int x = 0; x < cols; x++)
            {
                //if(newFilledPixels[y, x]==0)
                {
                    if (filledPixels[y, x] == 1)
                    {
                        countFilledPixelsInRow++;
                        if (startX == -1)
                        {
                            startX = x; // Устанавливаем начальный индекс
                        }
                        endX = x; // Обновляем конечный индекс
                    }
                }
            }

            if (countFilledPixelsInRow > 10)
            {
                for (int x = startX + 1; x < endX; x++) // Изменено на startX + 1 и x < endX
                {
                    if (newFilledPixels[y, x] == 0)
                    {
                        if (filledPixels[y, x] == 1)
                        {
                            newFilledPixels[y, x] = 1;
                            // Примените fillTexture к текущему пикселю
                            Color textureColor = texture.GetPixel(x - startX, y);
                            byte redByte = (byte)(textureColor.r * 255);
                            byte greenByte = (byte)(textureColor.g * 255);
                            byte blueByte = (byte)(textureColor.b * 255);
                            rgbData[0] = redByte;
                            rgbData[1] = greenByte;
                            rgbData[2] = blueByte;
                            byte[] rgbaData = { rgbData[0], rgbData[1], rgbData[2], (byte)255 };
                            processedResult.put(y, x, rgbaData);
                        }
                    }
                }
            }
        }

        ShowImage(processedResult);

        return processedResult;
    }
    void ShowImage(Mat mat)
    {
        opencvprocessed = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, opencvprocessed);
        Imgproc.circle(mat, GetPoint(), 10, circleColor, -1); // Рисует круг
      

        MeshObject.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", opencvprocessed);
        originalTexture= opencvprocessed;
        ///Debug.Log(rend.name);
        ///rend.material.SetTexture("_BaseMap", opencvprocessed);
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
