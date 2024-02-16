using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.Networking;
using System.Net;

public class ForceAcceptAll : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
public class WallPaint : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    int flip;
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject plane2;
    [SerializeField] private float ScaleDifference = 10; // a plane is 10 unit larger then a cube
    public Texture2D roomTexture;
    public TestScan TestScan ;
    int[,] newFilledPixels;
    int[,] newFilledPixels2;
    [SerializeField] private Texture2D texture2;
    public Vector2 vector2;
    public Texture2D resizedTexture;
    private Point topLeftPoint; // Define these as class fields
    private Point bottomRightPoint;
    public GameObject podlos;
    
    public Texture2D[] textures = new Texture2D[50];
    int save;
    public  Mat imgMat;
    public Texture2D originalTexture;
    public Texture2D trans;
    Mat changedRegions;
    // Reference to the material whose texture you want to change
    public Material targetMaterial;
    public Texture2D txr;
     private bool certificateValidationBypassed = false;
    static List<Point> filledPixels = new List<Point>();
    public MeshRenderer sourceMeshRenderer; // Меш, с которого копируется текстура
    public MeshRenderer targetMeshRenderer; // Меш, на который применяется текстура
     bool capitured;
   public  double cannyMinThres=140;
   public Mat mats;
   public Scalar scalrs;
    public double thresholdFactor = 10.0;

    public Slider sl;
    public bool dark;
     public void Reload()
    {
        Application.LoadLevel("Market");
    }
   public void AR()
    {
        Application.LoadLevel("ar");
    }

    // Start is called before the first frame update
    void Start()
    {
         //textures = new Texture2D[50];
        //float height = 2f * _camera.orthographicSize; // height is always twice the orthographic size
        //float width = height * _camera.aspect; // aspect ratio = screen width / screen height
        //plane.transform.localScale = new Vector3(width / ScaleDifference, height / ScaleDifference, 1);
        //imgMat = new Mat(roomTexture.height, roomTexture.width, CvType.CV_8UC4);
        //Utils.texture2DToMat(roomTexture, imgMat);
       // Установите ServerCertificateValidationCallback в true для обхода проверки сертификата
        newFilledPixels = new int[1024,1024];
        newFilledPixels2 = new int[1024,1024];
        ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;


        // Теперь вы можете выполнять запросы к серверу с недоверенным сертификатом без ошибок проверки
        try
        {
            using (var client = new WebClient())
            {
                string result = client.DownloadString("https://www.evrogroup.isp.regruhosting.ru/");
                Debug.Log("Server Response: " + result);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        };
    }
    public void GetTexture(string textureURL)
    {
       StartCoroutine(LoadTexture(textureURL));
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
			TestScan.texture2=loadedTexture;
        }
        else
        {
           Debug.Log("ОШИБКА НЕТ ТЕКТУРЫ");
        }
    }
    

IEnumerator LoadTexture(string textureURL)
{
    UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureURL);
var cert = new ForceAcceptAll();
 
// www is a UnityWebRequest
www.certificateHandler = cert;
 
// Send
 
cert?.Dispose();
    yield return www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.Success)
    {
        // Получаем текстуру из UnityWebRequest
        Texture2D loadedTexture = DownloadHandlerTexture.GetContent(www);

        // Устанавливаем текстуру в Renderer
        texture2 = loadedTexture;
		TestScan.texture2=loadedTexture;
    }
    else
    {
        Debug.LogError("Не удалось загрузить текстуру: " + www.error);
    }
    
    // Make sure to return null or use 'yield break' to end the coroutine
  
}
   
    public void Val()
    {
        cannyMinThres=(double)sl.value;
    }
public void Copy()
{
    capitured = true;
    if (sourceMeshRenderer == null || targetMeshRenderer == null)
    {
        Debug.LogError("Assign both source and target mesh renderers!");
        return;
    }

    // Получаем текстуру с исходного меша (должна быть Texture2D)
    Texture2D sourceTexture = sourceMeshRenderer.material.mainTexture as Texture2D;

    if (sourceTexture != null)
    {
        // Получаем исходные размеры текстуры
        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;

        // Вычисляем новые размеры текстуры (сжатие в два раза)
        int newWidth = sourceWidth ;
        int newHeight = sourceHeight;

        // Создаем новую Texture2D с новыми размерами
        Texture2D newTexture = new Texture2D(newWidth, newHeight);

        // Получаем пиксели из sourceTexture
        Color[] sourcePixels = sourceTexture.GetPixels();

        // Инициализируем массив для новых пикселей
        Color[] newPixels = new Color[newWidth * newHeight];
/* 
        // Итерируемся через новую текстуру и устанавливаем ее пиксели
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Вычисляем соответствующий пиксель из исходной текстуры
                int sourceX = x * 2;
                int sourceY = y * 2;

                // Усредняем цвета четырех пикселей исходной текстуры для уменьшения размера
                Color averageColor = (sourcePixels[sourceY * sourceWidth + sourceX] +
                                      sourcePixels[sourceY * sourceWidth + sourceX + 1] +
                                      sourcePixels[(sourceY + 1) * sourceWidth + sourceX] +
                                      sourcePixels[(sourceY + 1) * sourceWidth + sourceX + 1]) / 4f;

                newPixels[y * newWidth + x] = averageColor;
            }
        }

        // Устанавливаем новые пиксели в новую текстуру
        newTexture.SetPixels(newPixels);
        newTexture.Apply();
*/
        // Применяем новую текстуру к целевому мешу
        targetMeshRenderer.material.mainTexture = sourceMeshRenderer.material.mainTexture;
      
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] =(Texture2D) sourceMeshRenderer.material.mainTexture;
        }
        Debug.Log("Texture compressed and applied successfully!");
    }
    else
    {
        Debug.LogError("Source mesh does not have a Texture2D!");
    }

    // Создаем объект Mat и конвертируем новую текстуру в Mat (предполагая, что это OpenCV)
    imgMat = new Mat(targetMeshRenderer.material.mainTexture.height, targetMeshRenderer.material.mainTexture.width, CvType.CV_8UC4);
    Debug.Log(targetMeshRenderer.material.mainTexture.height);
    Debug.Log(targetMeshRenderer.material.mainTexture.width);
    Utils.texture2DToMat((Texture2D)targetMeshRenderer.material.mainTexture, imgMat);
}
void ReplaceColorWithTransparency(Mat mat, Scalar colorToReplace, double alphaValue)
{
    int rows = mat.rows();
    int cols = mat.cols();
    byte[] rgbaData = new byte[4];

    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < cols; x++)
        {
            mat.get(y, x, rgbaData);

            if (rgbaData[0] == colorToReplace.val[0] &&
                rgbaData[1] == colorToReplace.val[1] &&
                rgbaData[2] == colorToReplace.val[2] &&
                rgbaData[3] == colorToReplace.val[3])
            {
                rgbaData[3] = (byte)(alphaValue * 255);
                mat.put(y, x, rgbaData);
            }
        }
    }
}



    // Update is called once per frame
    void Update()
    {
        //if(capitured)

        if (Input.GetMouseButtonDown(0))
        {
      
     
            // Choosen color is red by default
         RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if(hit.transform.tag.Contains("tar"))
            {
            if(textures[save]!=null)
            {
            imgMat = new Mat(targetMeshRenderer.material.mainTexture.height, targetMeshRenderer.material.mainTexture.width, CvType.CV_8UC4);
            Texture2D txt= (Texture2D)(targetMeshRenderer.material.mainTexture);
            Utils.texture2DToMat(txt, imgMat);
            }            
            PaintWall(imgMat, GetPoint(), new Size(targetMeshRenderer.material.mainTexture.width, targetMeshRenderer.material.mainTexture.height), new Scalar(0, 255, 0, 255));
            }
        }
        }
    }
public Material displayMaterial; // Assign the material with the shader you want to use (e.g., Unlit/Texture)

   
 

bool IsPixelDark(Texture2D tex, Vector2 pixelUV)
{
    // Получаем цвет пикселя в UV-координатах
    Color pixelColor = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
    
    // Вычисляем среднюю интенсивность цвета пикселя (может потребоваться настройка порога)
    float intensity = (pixelColor.r + pixelColor.g + pixelColor.b) / 3.0f;
    
    // Если интенсивность менее 0.5, считаем пиксель темным, иначе светлым
    return intensity < 0.5f;
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

            // Проверяем, является ли пиксель темным и устанавливаем dar в зависимости от этого
            dark = IsPixelDark(tex, pixelUV);
        }
    }
    return point;
}
    Mat PaintWall(Mat image, Point p, Size imageSize, Scalar chosenColor)
    {
        
        double ratio = 2;
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
         mGreyScaleMat.convertTo( mGreyScaleMat, CvType.CV_8UC1, scale, -minMaxResult.minVal * scale);
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
    if (dark ==false) 
    {
        cannyMinThres =50;
    }
    
    else 
    {
        cannyMinThres =100;
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
            new OpenCVForUnity.CoreModule.Rect(0,0,0,0),
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
      
        result = ProcessResult(result, chosenColor,110,texture2);
        mats=result;
        scalrs=chosenColor;
        return result;
    }
public void Get()
{
 
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
    Mat processedResult = new Mat(targetMeshRenderer.material.mainTexture.height, targetMeshRenderer.material.mainTexture.width, CvType.CV_8UC4);
    Utils.texture2DToMat(textures[save], processedResult);

    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < cols; x++)
        {
            result.get(y, x, rgbData);
            processedResult.get(y, x, rgbData2);
            bool colorMatch = true;
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(rgbData[i] - colorToRemove.val[i]) >100)
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
                    newFilledPixels2[y, x]=1;
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
           if(newFilledPixels[y,x]==0)
           {
          if (filledPixels[y, x] == 1)
          {
           newFilledPixels[y,x]=1;
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

Mat ProcessResult2(Mat result, Scalar colorToRemove, int tolerance, Texture2D texture)
{
   
    int rows = result.rows();
    int cols = result.cols();
    byte[] rgbData = new byte[3];

    if (changedRegions == null)
    {
        // Если changedRegions не был создан, создайте его при первом вызове.
        changedRegions = new Mat(rows, cols, CvType.CV_8UC1, new Scalar(0)); // Инициализируйте пустой матрицей.
    }

    Mat processedResult = new Mat(rows, cols, CvType.CV_8UC4);

    for (int y = 0; y < rows; y++)
    {
        for (int x = 0; x < cols; x++)
        {
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

            byte[] rgbaData;
            if (colorMatch || changedRegions.get(y, x)[0] != 0)
            {
                Color textureColor = texture.GetPixel(x, y);

                byte redByte = (byte)(textureColor.r * 255);
                byte greenByte = (byte)(textureColor.g * 255);
                byte blueByte = (byte)(textureColor.b * 255);

                rgbaData = new byte[] { redByte, greenByte, blueByte, (byte)255 };
                // Пометьте область в changedRegions как измененную.
                changedRegions.put(y, x, new byte[] { 1 });
            }
            else
            {
                // Если цвет не совпадает и область не помечена, установите прозрачный цвет.
                rgbaData = new byte[] { 0, 0, 0, 0 };
            }

            processedResult.put(y, x, rgbaData);
        }
    }

    return processedResult;
}




public void Ret()
{
    save=save-1;
    targetMeshRenderer.material.mainTexture = textures[save];
}
void ShowImage(Mat mat)
{
    save=save+1;
    textures[save] = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);

    ////////////////////////////////////
    Utils.matToTexture2D(mat, textures[save]);
    plane.GetComponent<Renderer>().material.mainTexture = textures[save];
    ////////////////////////////////////
    
   
}
void ShowImage2(Mat mat)
{
   
   
   
}
}
