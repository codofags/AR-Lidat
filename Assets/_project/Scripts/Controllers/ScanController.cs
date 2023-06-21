using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private ScanMesh _scanMesh;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _viewPanel;
    [SerializeField] private Transform _modelViewParent;

    private bool _isScanning;

    protected override void Awake()
    {
        base.Awake();
        _arMeshManager.enabled = false;
        //_planeManager. enabled = false;
    }

    public void ScanStart()
    {
        if (!_isScanning)
        {
            StartCoroutine(Scaning());
            _arMeshManager.enabled = true; // Включаем ARMeshManager для сканирования мешей

            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // Получаем доступ к подсистеме ARKitMeshSubsystem

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Start();
                _isScanning = true;
            }
        }
    }

    IEnumerator Scaning()
    {
        yield return new WaitForSeconds(_scanningTime);
        if (!_isScanning)
            yield break;

        _isScanning = false;
        ScanStop();
        //var sccreenShot = ScreenCapture.CaptureScreenshotAsTexture();
        //yield return new WaitForSeconds(2f);
        //foreach (var meshFilter in _arMeshManager.meshes)
        //{
        //    meshFilter.GetComponent<MeshRenderer>().enabled = true;
        //    meshFilter.TextureMesh(sccreenShot);
        //}
        Debug.Log("Done");
    }

    public void ScanStop()
    {
        if (_isScanning)
        {
            //Camera.main.enabled = false;
            _arMeshManager.enabled = false; // Отключаем ARMeshManager

            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Stop();
                //_arCameraManager.enabled = false;
                _isScanning = false;
            }
            _arCameraManager.enabled = false;
            UIController.Instance.ShowViewerPanel();
            foreach(var meshFilter in _arMeshManager.meshes)
            {
                meshFilter.transform.SetParent(_modelViewParent, false);
            }
        }
    }

    public void ConvertToModel()
    {
        _scanMesh.SetTextures();
    }
}
