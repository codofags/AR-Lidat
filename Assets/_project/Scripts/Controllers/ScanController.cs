using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _viewPanel;
    [SerializeField] private GameObject _modelViewer;
    [SerializeField] private Transform _modelViewParent;

    private bool _isScanning = false;
    private List<MeshData> _datas = new List<MeshData>();
    private float _getScreenTime = 1f;
    private float _getScreenTimeTemp = 0;

    protected override void Awake()
    {
        base.Awake();
        _arMeshManager.enabled = false;
    }

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    private void Update()
    {
        _getScreenTimeTemp += Time.deltaTime;
    }

    public void ScanStart()
    {
        if (!_isScanning)
        {
            _arMeshManager.enabled = true; // Включаем ARMeshManager для сканирования мешей

            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // Получаем доступ к подсистеме ARKitMeshSubsystem

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Start();
                _isScanning = true;
                StartCoroutine(Scaning());
                Debug.Log("Scan START");
            }
        }
    }

    IEnumerator Scaning()
    {
        yield return new WaitForSeconds(_scanningTime);
        if (!_isScanning)
            yield break;

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

            _arCameraManager.enabled = false;
            UIController.Instance.ShowViewerPanel();

            foreach(var meshFilter in _arMeshManager.meshes)
            {
                meshFilter.transform.SetParent(_modelViewParent, false);
            }

            _modelViewer.SetActive(true);

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Stop();
                //_arCameraManager.enabled = false;
                _isScanning = false;
                Debug.Log("Scan STOP");
            }
        }
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            CreateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            UpdateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            RemoveMeshObject(meshFilter);
        }
    }

    private void CreateMeshObject(MeshFilter meshFilter)
    {
        SaveCameraTextureToMesh(meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        SaveCameraTextureToMesh(meshFilter);
    }

    private void RemoveMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        var data = _datas.FirstOrDefault(data => data.MeshFilter == meshFilter);
        if (data != null)
            _datas.Remove(data);

        Destroy(meshFilter.gameObject);
    }

    private void SaveCameraTextureToMesh(MeshFilter meshFilter)
    {
        if (_getScreenTimeTemp > _getScreenTime)
            return;

        _getScreenTimeTemp = 0f;
        ToogleMeshes(false);
        UIController.Instance.HideUI();
        var data = _datas.FirstOrDefault((data) => data.MeshFilter == meshFilter);

        if (data != null)
        {
            data.Texture = ScreenCapture.CaptureScreenshotAsTexture();
            Debug.Log("Save Screen");
        }
        else
        {
            data = new MeshData(meshFilter, ScreenCapture.CaptureScreenshotAsTexture());
        }


        UIController.Instance.ShowUI();
        ToogleMeshes(true);
    }

    public void ConvertToModel()
    {
        SetTextures();
    }

    public void SetTextures()
    {
        foreach (var data in _datas)
        {
            data.MeshFilter.TextureMesh(data.Texture);
        }
    }

    private void ToogleMeshes(bool activate)
    {
        if (_arMeshManager.meshes == null || _arMeshManager.meshes.Count <= 0)
            return;

        foreach (var mesh in _arMeshManager.meshes)
        {
            mesh.GetComponent<MeshRenderer>().enabled = activate;
        }
    }
}
