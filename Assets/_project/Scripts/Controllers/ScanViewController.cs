//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using UnityEngine.EventSystems;

//public class ScanViewController : MonoBehaviour
//{
//    public enum ScanMode
//    {
//        Noneed,
//        Doing,
//        Done
//    }

//    public ARSession arSession;
//    public ARMeshManager arMeshManager;
//    public ARCameraManager arCameraManager;
//    public ARPlaneManager arPlaneManager;
//    public ARPointCloudManager arPointCloudManager;
//    public ARSessionOrigin arSessionOrigin;
//    public ARRaycastManager arRaycastManager;
//    public GameObject scanButton;
//    public GameObject resetButton;

//    public LabelScene labelScene;
//    public ScanMode scanMode = ScanMode.Noneed;
//    public Texture2D originalSource;

//    private void Start()
//    {
//        arMeshManager.meshPrefab.SetActive(false);
//        labelScene.SetText("Scan");
//    }

//    public void RotateMode()
//    {
//        switch (scanMode)
//        {
//            case ScanMode.Noneed:
//                scanMode = ScanMode.Doing;
//                labelScene.SetText("Reset");
//                originalSource = arCameraManager.cameraMaterial.GetTexture("_MainTex") as Texture2D;
//                arCameraManager.cameraMaterial.SetTexture("_MainTex", null);
//                arCameraManager.startingMode = ARCameraManager.CameraMode.PlainTexture;
//                arCameraManager.requestedCameraMode = ARCameraManager.CameraMode.PlainTexture;
//                arCameraManager.enabled = true;
//                arCameraManager.StartCamera();
//                break;
//            case ScanMode.Doing:
//                break;
//            case ScanMode.Done:
//                scanAllGeometry(false);
//                scanMode = ScanMode.Noneed;
//                labelScene.SetText("Scan");
//                arCameraManager.cameraMaterial.SetTexture("_MainTex", originalSource);
//                arCameraManager.startingMode = ARCameraManager.CameraMode.Environment;
//                arCameraManager.requestedCameraMode = ARCameraManager.CameraMode.Environment;
//                arCameraManager.enabled = true;
//                arCameraManager.StartCamera();
//                break;
//        }
//    }

//    public void SetLabelPosition(Vector2 position)
//    {
//        labelScene.transform.position = position;
//    }

//    private void Update()
//    {
//        if (scanMode == ScanMode.Doing)
//        {
//            scanAllGeometry(true);
//            scanMode = ScanMode.Done;
//        }
//    }

//    public void OnMeshGenerated(ARMeshUpdatedEventArgs eventArgs)
//    {
//        ARMeshFilter meshFilter = eventArgs.mesh.GetComponent<ARMeshFilter>();
//        meshFilter.mesh = eventArgs.mesh;
//    }

//    public void OnMeshRemoved(ARMeshRemovedEventArgs eventArgs)
//    {
//        Destroy(eventArgs.mesh);
//    }

//    public void OnMeshUpdated(ARMeshUpdatedEventArgs eventArgs)
//    {
//        ARMeshFilter meshFilter = eventArgs.mesh.GetComponent<ARMeshFilter>();
//        meshFilter.mesh = eventArgs.mesh;
//    }

//    public void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
//    {
//        XRCameraImage image;
//        if (arCameraManager.TryGetLatestImage(out image))
//        {
//            int width = image.width;
//            int height = image.height;
//            Texture2D cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
//            cameraTexture.LoadRawTextureData(image.GetPlane(0).data);
//            cameraTexture.Apply();

//            if (scanMode == ScanMode.Doing)
//            {
//                arCameraManager.startingMode = ARCameraManager.CameraMode.Environment;
//                arCameraManager.requestedCameraMode = ARCameraManager.CameraMode.Environment;
//                arCameraManager.enabled = true;
//                arCameraManager.StartCamera();
//                scanAllGeometry(false);
//                scanMode = ScanMode.Noneed;
//                labelScene.SetText("Scan");
//                arCameraManager.cameraMaterial.SetTexture("_MainTex", originalSource);
//            }

//            image.Dispose();
//        }
//    }

//    private void scanAllGeometry(bool needTexture)
//    {
//        foreach (ARMeshFilter meshFilter in FindObjectsOfType<ARMeshFilter>())
//        {
//            meshFilter.gameObject.SetActive(needTexture);
//        }
//    }
//}
