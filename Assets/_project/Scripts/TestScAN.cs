using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgprocModule;
public class TestScAN : MonoBehaviour
{
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _cameraManager;

    private Camera _camera;
    private int _textureDevider = 1;
    public Texture2D opencvprocessed;

    private void Start()
    {
        _camera = Camera.main;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;
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
    private async void Update()
    {
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
                    var tex =  GetAndProcessImageAsync();
                    //Тут нужно делать твои действия с OpenCV

                    Mat imgMat = new Mat(tex.height, tex.width, CvType.CV_8UC4);
                    Utils.texture2DToMat(tex, imgMat);
              
                    Paint(imgMat, GetPoint(), new Size(tex.width, tex.height), new Scalar(255, 0, 0, 255));

                    // Создаем новый материал.
                    Material newMaterial = new Material(Shader.Find("Standard"));
                    newMaterial.color = Color.white;
                    if (opencvprocessed != null)
                    {
                        newMaterial.SetTexture("_BaseMap", opencvprocessed);
                    }
                    else
                    {
                        newMaterial.SetTexture("_BaseMap", tex);
                    }
                    var mf = meshCol.GetComponent<MeshFilter>();
                    ///GenerateSimpleUV(tex, ref mf);
                    var render = mf.GetComponent<MeshRenderer>();
                    render.material = newMaterial;


                    _arMeshManager.enabled = false; // Отключаем ARMeshManager

                    XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

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
        double cannyMinThres = 30.0;
        double ratio = 2.5;
        Mat mRgbMat = image;
        Imgproc.cvtColor(mRgbMat, mRgbMat, Imgproc.COLOR_RGBA2RGB);
        Mat mask = new Mat(new Size(mRgbMat.cols() / 8.0f, mRgbMat.rows() / 8.0f), CvType.CV_8UC1, new Scalar(0.0));
        Mat img = new Mat();
        mRgbMat.copyTo(img);
         Mat mGreyScaleMat = new Mat();
        Imgproc.cvtColor(mRgbMat, mGreyScaleMat, Imgproc.COLOR_RGB2GRAY, 3);
        Imgproc.medianBlur(mGreyScaleMat, mGreyScaleMat, 3);
        Mat cannyGreyMat = new Mat();
        Imgproc.Canny(mGreyScaleMat, cannyGreyMat, cannyMinThres, cannyMinThres * ratio, 3);
        Mat hsvImage = new Mat();
        Imgproc.cvtColor(img, hsvImage, Imgproc.COLOR_RGB2HSV);
        List<Mat> list = new List<Mat>(3);
        Core.split(hsvImage, list);
        Mat sChannelMat = new Mat();
        List<Mat> slist = new List<Mat>();
        slist.Add(list[1]);
        Core.merge(slist, sChannelMat);
         Mat cannyMat = new Mat();
        Imgproc.Canny(sChannelMat, cannyMat, cannyMinThres, cannyMinThres * ratio, 3);
        Core.addWeighted(cannyMat, 0.5, cannyGreyMat, 0.5, 0.0, cannyMat);
        Imgproc.dilate(cannyMat, cannyMat, mask, new Point(0.0, 0.0), 5);
        double width = imageSize.width;
        double height = imageSize.height;
        Point seedPoint = p;
        Imgproc.resize(cannyMat, cannyMat, new Size(cannyMat.width() + 2.0, cannyMat.height() + 2.0));
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
        Mat rgbHsvImage = new Mat();
        Imgproc.cvtColor(mRgbMat, rgbHsvImage, Imgproc.COLOR_RGB2HSV);
        List<Mat> list1 = new List<Mat>(3);
        Core.split(rgbHsvImage, list1);
        Mat result = new Mat();
        List<Mat> newList = new List<Mat>();
        newList.Add(list1[0]);
        newList.Add(list1[1]);
        newList.Add(list1[2]);
        Core.merge(newList, result);
        Imgproc.cvtColor(result, result, Imgproc.COLOR_HSV2RGB);
        Core.addWeighted(result, 0.7, img, 0.3, 0.0, result);
        ShowImage(result);
        return result;
    }
    void ShowImage(Mat mat)
    {
        opencvprocessed = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(mat, opencvprocessed);
        
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
