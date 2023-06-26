using CoolishUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private float _rotationThreshold = 30f;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private MeshSlicer _slicer;
    [SerializeField] private GameObject _modelViewer;
    [SerializeField] private Transform _modelViewParent;
    [SerializeField] private SimpleDebugConsole _console;

    private bool _isScanning = false;
    private List<MeshData> _datas = new List<MeshData>();
    private Coroutine _scanning;
    private Quaternion initialRotation;

    private List<ScanData> _scans = new List<ScanData>();
    public Action OnStart;
    public Action OnStop;

    private List<MeshRenderer> _meshParts;

    protected override void Awake()
    {
        base.Awake();
        _arMeshManager.enabled = false;
        initialRotation = _arCameraManager.GetCameraPose().rotation;
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
        if (!_isScanning)
            return;

        var pose = _arCameraManager.GetCameraPose();
        Quaternion rotationDelta = Quaternion.Inverse(initialRotation) * pose.rotation;

        // Получаем угол поворота вокруг оси Y
        float rotationAngleY = Mathf.Abs(rotationDelta.eulerAngles.y);
        // Проверяем, превышает ли угол поворота пороговое значение
        if (rotationAngleY > _rotationThreshold)
        {
            var data = new ScanData(pose,_arCameraManager.GetCameraTexture());
            _scans.Add(data);
            // Сбрасываем начальный поворот камеры
            initialRotation = pose.rotation;
        }
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
                OnStart?.Invoke();
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

        Debug.Log("Time! Scan Stopping!");
        ScanStop();
    }

    public void ScanStop()
    {
        if (_isScanning)
        {
            _arMeshManager.enabled = false; // Отключаем ARMeshManager
            Debug.Log("Scan STOP!");

            foreach (var meshFilter in _arMeshManager.meshes)
            {
                meshFilter.transform.SetParent(_modelViewParent, false);
            }
            OnStop?.Invoke();
            _modelViewer.gameObject.SetActive(true);
            UIController.Instance.ShowViewerPanel();
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
        Debug.Log($"Mesh create. {_arMeshManager.meshes.Count}");
        meshFilter.GetComponent<MeshRenderer>().material.color = Extensions.GetRandomColor();
        //SaveCameraTextureToMesh(meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        //SaveCameraTextureToMesh(meshFilter);
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }
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

    IEnumerator SaveScreen(MeshFilter meshFilter)
    {
        ToogleMeshes(false);
        UIController.Instance.HideUI();
        var screenShoot = ScreenCapture.CaptureScreenshotAsTexture();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        var data = _datas.FirstOrDefault((data) => data.MeshFilter == meshFilter);
        if (data != null)
        {
            data.Texture = screenShoot;
            Debug.Log("Update Screen");
        }
        else
        {
            data = new MeshData(meshFilter, screenShoot);
            Debug.Log("Create Screen");
            _datas.Add(data);
        }


        UIController.Instance.ShowUI();
        ToogleMeshes(true);
        _scanning = null;
    }

    private void SaveCameraTextureToMesh(MeshFilter meshFilter)
    {
        Debug.Log("Save Screen");
        if (_scanning == null)
            _scanning = StartCoroutine(SaveScreen(meshFilter));
    }

    public void ConvertToModel()
    {
        SetTextures();
    }

    public void MeshesSlice()
    {
        _meshParts = new List<MeshRenderer>();
        foreach (var meshFilter in _arMeshManager.meshes)
        {
            _meshParts.AddRange(_slicer.SliceMeshIntoCubes(meshFilter));
        }
    }

    public void SetTextures()
    {
        if (_datas == null || _datas.Count == 0)
        {
            Debug.Log("No DATAS");
            return;
        }

        Debug.Log($"Convert Meshes: {_arMeshManager.meshes.Count}. Datas: {_datas.Count}");
        foreach (var data in _datas)
        {
            data.MeshFilter.TexturedMesh(data.Texture);
        }
    }

    private void ToogleMeshes(bool activate)
    {
        _console.enabled = activate;
        if (_arMeshManager.meshes == null || _arMeshManager.meshes.Count <= 0)
            return;

        foreach (var mesh in _arMeshManager.meshes)
        {
            mesh.GetComponent<MeshRenderer>().enabled = activate;
        }
    }
}
