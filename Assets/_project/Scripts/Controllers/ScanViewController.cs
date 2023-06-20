using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.Diagnostics.Tracing;

public class ScanViewController : MonoBehaviour
{
    public enum ScanMode
    {
        Noneed,
        Doing,
        Done
    }

    public ARSession arSession;
    public ARMeshManager arMeshManager;
    public ARCameraManager arCameraManager;
    public ARPlaneManager arPlaneManager;
    public ARPointCloudManager arPointCloudManager;
    public ARSessionOrigin arSessionOrigin;
    public ARRaycastManager arRaycastManager;
    public GameObject scanButton;
    public GameObject resetButton;

    public LabelScene labelScene;
    public ScanMode scanMode = ScanMode.Noneed;
    public Texture2D originalSource;

    private void Start()
    {
        arMeshManager.meshesChanged += MeshesChanged;
        arMeshManager.enabled = false;
        labelScene.SetText("Scan");
    }

    private void MeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            OnMeshGenerated(meshFilter);
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            OnMeshUpdated(meshFilter);
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            OnMeshRemoved(meshFilter);
        }
    }

    public void RotateMode()
    {
        switch (scanMode)
        {
            case ScanMode.Noneed:
                scanMode = ScanMode.Doing;
                labelScene.SetText("Reset");
                originalSource = arCameraManager.cameraMaterial.GetTexture("_MainTex") as Texture2D;
                arCameraManager.cameraMaterial.SetTexture("_MainTex", null); 
                arCameraManager.subsystem.requestedCamera = Feature.AnyCamera;
                arCameraManager.enabled = true;
                arCameraManager.subsystem.Start();
                break;
            case ScanMode.Doing:
                break;
            case ScanMode.Done:
                scanAllGeometry(false);
                scanMode = ScanMode.Noneed;
                labelScene.SetText("Scan");
                arCameraManager.cameraMaterial.SetTexture("_MainTex", originalSource);
                arCameraManager.enabled = true;
                arCameraManager.subsystem.Start();
                break;
        }
    }

    public void SetLabelPosition(Vector2 position)
    {
        labelScene.transform.position = position;
    }

    private void Update()
    {
        if (scanMode == ScanMode.Doing)
        {
            scanAllGeometry(true);
            scanMode = ScanMode.Done;
        }
    }

    public void OnMeshGenerated(MeshFilter meshFilter)
    {
    }

    public void OnMeshRemoved(MeshFilter meshFilter)
    {
        Destroy(meshFilter.gameObject);
    }

    public void OnMeshUpdated(MeshFilter meshFilter)
    {
    }

    public void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (arCameraManager.TryAcquireLatestCpuImage(out var image))
        {
            int width = image.width;
            int height = image.height;
            Texture2D cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            cameraTexture.LoadRawTextureData(image.GetPlane(0).data);
            cameraTexture.Apply();

            if (scanMode == ScanMode.Doing)
            {
                arCameraManager.enabled = true;
                arCameraManager.subsystem.Start();
                scanAllGeometry(false);
                scanMode = ScanMode.Noneed;
                labelScene.SetText("Scan");
                arCameraManager.cameraMaterial.SetTexture("_MainTex", originalSource);
            }

            image.Dispose();
        }
    }

    private void scanAllGeometry(bool needTexture)
    {
        foreach (MeshFilter meshFilter in FindObjectsOfType<MeshFilter>())
        {
            meshFilter.gameObject.SetActive(needTexture);
        }
    }
}
