using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private ARMeshManager _meshManager;

    protected override void Awake()
    {
        base.Awake();
        //_planeManager. enabled = false;
    }

    public void ScanStart()
    {
        _meshManager.meshesChanged += OnMeshesChanged;
        //_planeManager.enabled = true;
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs obj)
    {
        throw new System.NotImplementedException();
    }

    public void ScanStop()
    {

    }
}
