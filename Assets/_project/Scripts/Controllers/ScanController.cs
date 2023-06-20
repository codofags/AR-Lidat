using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private ScanMesh _scanMesh;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _viewPanel;

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
            _arMeshManager.enabled = true; // Включаем ARMeshManager для сканирования мешей

            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // Получаем доступ к подсистеме ARKitMeshSubsystem

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Start();
                _isScanning = true;
            }
        }
    }

    public void ScanStop()
    {
        if (_isScanning)
        {
            Camera.main.enabled = false;
            _arMeshManager.enabled = false; // Отключаем ARMeshManager

            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Stop();
                _arCameraManager.enabled = false;
                _isScanning = false;
            }
            _viewPanel.SetActive(true);
        }
    }
}
