using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class TextureFloodFill : MonoBehaviour
{
    public RawImage canvasRawImage;
    public Button extractTextureButton;
    public GameObject sq;

    private Texture2D canvasTexture;
    private Mat canvasMat;
    public MeshRenderer sourceMeshRenderer; 
    public MeshRenderer targetMeshRenderer; 
    private void CopyTextureFromSourceToTarget()
    {
        if (sourceMeshRenderer != null && targetMeshRenderer != null)
        {
            Material sourceMaterial = sourceMeshRenderer.sharedMaterial;
            Material targetMaterial = targetMeshRenderer.material;

            if (sourceMaterial != null && targetMaterial != null)
            {
                targetMaterial.mainTexture = sourceMaterial.mainTexture;
            }
        }
    }

    // Вызывайте эту функцию, чтобы скопировать текстуру
    public void CopyTexture()
    {
        CopyTextureFromSourceToTarget();
    }

    private void Start()
    {
        canvasTexture = canvasRawImage.texture as Texture2D;
        canvasMat = new Mat(canvasTexture.height, canvasTexture.width, CvType.CV_8UC4);
        //extractTextureButton.onClick.AddListener(ExtractTexture);
        Texture2D imgTexture =canvasRawImage.texture as Texture2D;
            
            Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC3);
            
            Utils.texture2DToMat (imgTexture, imgMat);
            Debug.Log ("imgMat.ToString() " + imgMat.ToString ());


            Imgproc.line (imgMat, new Point (50, 50), new Point (400, 105), new Scalar (0, 0, 200), 3);  

            ///
            Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
            Utils.matToTexture2D (imgMat, texture);
            FloodFillSquare(0, 0, 32);
            sq.GetComponent<Renderer> ().material.mainTexture = texture;
    }
public void Copymat()
{
    
}



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localClickPos = GetLocalClickPosition();

            int x = Mathf.RoundToInt(localClickPos.x);
            int y = Mathf.RoundToInt(localClickPos.y);

            if (IsInsideTextureBounds(x, y))
            {
                DrawPoint(x, y);
                
            }
        }
    }

    private Vector2 GetLocalClickPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRawImage.rectTransform, Input.mousePosition, null, out Vector2 localPoint);
        return localPoint + new Vector2(canvasTexture.width / 2, canvasTexture.height / 2);
    }

    private bool IsInsideTextureBounds(int x, int y)
    {
        return x >= 0 && x < canvasTexture.width && y >= 0 && y < canvasTexture.height;
    }

    private void DrawPoint(int x, int y)
    {
        Color32[] canvasPixels = canvasTexture.GetPixels32();
        int index = y * canvasTexture.width + x;

        if (index >= 0 && index < canvasPixels.Length)
        {
            canvasPixels[index] = Color.red;  // Change color to red for drawing points
            canvasTexture.SetPixels32(canvasPixels);
            canvasTexture.Apply();
        }
    }


private void FloodFillSquare(int centerX, int centerY, int size)
{
    Utils.texture2DToMat(canvasTexture, canvasMat);

    Mat mask = new Mat(canvasTexture.height + 2, canvasTexture.width + 2, CvType.CV_8UC1, new Scalar(0));
    Imgproc.cvtColor(canvasMat, mask, Imgproc.COLOR_RGBA2GRAY);

    Mat filledArea = mask.clone();
    Imgproc.rectangle(filledArea, new Point(centerX - size / 2, centerY - size / 2), new Point(centerX + size / 2, centerY + size / 2), new Scalar(255), -1);

    Mat newMask = new Mat();
    Core.subtract(mask, filledArea, newMask);

    Scalar newVal = new Scalar(0, 0, 0, 0);  // Change color to fill with (red in this case)

    Imgproc.floodFill(canvasMat, newMask, new Point(centerX, centerY), newVal, new OpenCVForUnity.CoreModule.Rect(),
                      new Scalar(20, 20, 20), new Scalar(20, 20, 20), Imgproc.FLOODFILL_FIXED_RANGE);

    Utils.matToTexture2D(canvasMat, canvasTexture);
    canvasTexture.Apply();
}
}