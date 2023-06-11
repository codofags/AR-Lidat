using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private ScanMesh _scanMesh;

    protected override void Awake()
    {
        base.Awake();
        //_planeManager. enabled = false;
    }

    public void ScanStart()
    {
        _scanMesh.gameObject.SetActive(true);
    }

    public void ScanStop()
    {
        _scanMesh.gameObject.SetActive(false);
    }
}
