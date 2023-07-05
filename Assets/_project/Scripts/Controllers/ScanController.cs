using CoolishUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private Camera _checkMeshCamera;
    [SerializeField] private MeshSlicer _slicer;
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _modelViewer;
    [SerializeField] private Transform _modelViewParent;
    [SerializeField] private float _getScreenTime = .5f;

    [SerializeField] private Transform _cameraViewPrefab;
    [SerializeField] private Material _nonWireframeMaterial;

    private bool _isScanning = false;
    private List<MeshData> _datas = new List<MeshData>();
    private float _getScreenTimeTemp = 1f;
    private Coroutine _scanning;

    [SerializeField] private MeshFilter[] TestMeshes;

    private List<GameObject> _slicedMeshes = new List<GameObject>();
    private Vector3 _initPos;
    private Quaternion _initRot;

    private static string SCAN_TEXT = "SCANNING";
    private static string MESH_CONVERT_TEXT = "Creating a Mesh.\r\nStatus: Generated";
    private static string MESH_TEXTURE_TEXT = "Texture Overlay.\r\nStatus: Not exported";

    protected override void Awake()
    {
        base.Awake();
        _isScanning = false;
        _getScreenTimeTemp = _getScreenTime;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;

        if (_arMeshManager.meshes != null && _arMeshManager.meshes.Count > 0)
            _arMeshManager.meshes.Clear();
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
            UIController.Instance.InfoPanel.Show(SCAN_TEXT);
            _arMeshManager.enabled = true; // Включаем ARMeshManager для сканирования мешей
            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // Получаем доступ к подсистеме ARKitMeshSubsystem

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Start();
                _isScanning = true;
                //StartCoroutine(Scaning());
                CameraPositionSaver.Instance.StartSaving();
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
            UIController.Instance.InfoPanel.Show(MESH_CONVERT_TEXT);
            CameraPositionSaver.Instance.StopSaving();

            StartCoroutine(Stopping());
            //Camera.main.enabled = false;
            //_arMeshManager.enabled = false; // Отключаем ARMeshManager

            //XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

            //ToogleMeshes(false);
            //UIController.Instance.HideUI();
            //var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            //foreach (var data in _datas)
            //{
            //    data.Texture = screenShot;
            ////}
            //ToogleMeshes(true);
            //UIController.Instance.ShowUI();

            //UIController.Instance.ShowViewerPanel();

            //foreach (var meshFilter in _arMeshManager.meshes)
            //{
            //    meshFilter.transform.SetParent(_modelViewParent, false);
            //}

            //_modelViewer.SetActive(true);
            //if (arMeshSubsystem != null)
            //{
            //    arMeshSubsystem.Stop();
            //    _arCameraManager.enabled = false;
            //    _isScanning = false;
            //    Debug.Log($"Scan STOP. Meshes: {_arMeshManager.meshes.Count} Datas: {_datas.Count}");
            //}
        }
    }

    IEnumerator Stopping()
    {
        Debug.Log(_arMeshManager == null);
        //UIController.Instance.HideUI();
        if (_arMeshManager != null)
            _arMeshManager.enabled = false; // Отключаем ARMeshManager

        XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

        if (arMeshSubsystem != null)
        {
            arMeshSubsystem.Stop();
            _isScanning = false;
        }

        yield return new WaitForSeconds(1);

        Debug.Log("step 1");

        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.transform.SetParent(_modelViewParent, false);

            var renderer = meshFilter.GetComponent<MeshRenderer>();
            renderer.material = _nonWireframeMaterial;
            renderer.material.color = Random.ColorHSV();
        }

        Debug.Log("step 2");

        var cameraDatas = CameraPositionSaver.Instance.SavedCameraData.Values.ToList();
        _checkMeshCamera.transform.parent = _modelViewParent;

        Debug.Log("step 3");

        foreach (var camPos in cameraDatas)
        {
            if (camPos.Texture == null)
                continue;

            var newCameraView = Instantiate(_cameraViewPrefab, _modelViewParent);
            newCameraView.localPosition = camPos.Position;
            newCameraView.localRotation = camPos.Rotation;
            newCameraView.localScale = Vector3.one * 0.1f;
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("step 4");
        var combinedObject = CombineMeshes(_arMeshManager.meshes);
        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.gameObject.SetActive(false);
        }

        _arCameraManager.enabled = false;

        yield return StartCoroutine(ConvertMeshes());
    }

    private IEnumerator ConvertMeshes()
    {
        Debug.Log("step 5");

        var combinedObject = CombineMeshes(_arMeshManager.meshes);
        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.gameObject.SetActive(false);
        }

        Debug.Log("WAIT 5 sec");
        yield return new WaitForSeconds(5f);

        Debug.Log("step 6");

        _slicedMeshes = _slicer.SliceMesh(combinedObject, _nonWireframeMaterial);
        Debug.Log($"Mesh count: {_slicedMeshes.Count}");

        foreach (var sMesh in _slicedMeshes)
        {
            sMesh.transform.SetParent(_modelViewParent, false);
        }

        var meshesGOs = _slicedMeshes.Select(mesh => mesh.GetComponent<MeshFilter>()).ToList();
        var mesh = CombineMeshes(meshesGOs);
        _initPos = mesh.transform.position;
        _initRot = mesh.transform.rotation;


        _modelViewer.gameObject.SetActive(true);
        UIController.Instance.ShowViewerPanel();

        var model = FindObjectOfType<ThirdPersonCamera>();
        if (model != null)
            model.IsInteractable = true;
        Debug.Log($"Meshes Load: {_slicedMeshes.Count}. DONE.");

    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            //CreateMeshObject(meshFilter);
            //meshFilter.GetComponent<MeshRenderer>().material.color = Color.green;
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            //UpdateMeshObject(meshFilter);
            //meshFilter.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            //RemoveMeshObject(meshFilter);
        }
    }

    private void CreateMeshObject(MeshFilter meshFilter)
    {
        Debug.Log($"Mesh create. {_arMeshManager.meshes.Count}");
        SaveCameraTextureToMesh(meshFilter);
    }

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        _getScreenTimeTemp += Time.deltaTime;
        if (_getScreenTimeTemp >= _getScreenTime)
        {
            SaveCameraTextureToMesh(meshFilter);
            _getScreenTimeTemp = 0f;
        }

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
        //SetTextures();

        UIController.Instance.HideViewer();
        StartCoroutine(Converting());
    }

    IEnumerator Converting()
    {
        var model = FindObjectOfType<ThirdPersonCamera>();
        if (model != null)
            model.IsInteractable = false;

        yield return null;
        var cameraDatas = CameraPositionSaver.Instance.SavedCameraData.Values.ToList();
        Debug.Log("WAIT 10 sec");
        yield return new WaitForSeconds(10f);

        var handledMeshes = new List<GameObject>();

        foreach (var camData in cameraDatas)
        {
            if (_slicedMeshes.Count == 0)
                break;

            int handledCount = 0;
            for (int i = 0; i < _slicedMeshes.Count; ++i)
            {
                if (_slicedMeshes[i].name.StartsWith("Handled"))
                    continue;

                var mf = _slicedMeshes[i].GetComponent<MeshFilter>();
                _checkMeshCamera.transform.localPosition = camData.Position;
                _checkMeshCamera.transform.localRotation = camData.Rotation;
                //IsMeshInCamera(mf, camData.Position, camData.Rotation))
                if (_checkMeshCamera.IsMeshFullyIn(mf))
                {
                    mf.GenerateUV(_checkMeshCamera, camData.Texture);
                    var render = mf.GetComponent<MeshRenderer>();
                    render.material = _nonWireframeMaterial;
                    render.material.color = Color.white;
                    render.material.SetTexture("_BaseMap", camData.Texture);

                    mf.name = $"Handled_{mf.name}";
                    ++handledCount;
                    handledMeshes.Add(mf.gameObject);
                }
                yield return new WaitForEndOfFrame();
            }

            Debug.Log($"CamData {camData.Id}: {handledCount} handled");
        }

        var another = _slicedMeshes.Where(mesh => !mesh.name.StartsWith("Handled"));

        if (another != null && another.Count() > 0)
        {
            foreach( var mesh in another)
            {
                Debug.Log($"NOT CAM FOR MESH {mesh.name}");
                var renderer = mesh.GetComponent<MeshRenderer>();
                renderer.material.color = Color.magenta;
                renderer.enabled = false;
            }
        }
        if (model != null)
            model.IsInteractable = true;

        UIController.Instance.InfoPanel.Show(MESH_TEXTURE_TEXT);
        UIController.Instance.ShowExportPanel();
        Debug.Log("DONE Converting");
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
        //_console.enabled = activate;
        if (_arMeshManager.meshes == null || _arMeshManager.meshes.Count <= 0)
            return;

        foreach (var mesh in _arMeshManager.meshes)
        {
            mesh.GetComponent<MeshRenderer>().enabled = activate;
        }
    }



    private MeshFilter CombineMeshes(IList<MeshFilter> meshFilters)
    {
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        int i = 0;
        while (i < meshFilters.Count)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);

        mesh.RecalculateNormals();
        var go = new GameObject("Combined");
        go.transform.parent = _modelViewParent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

        var resultFilter = go.AddComponent<MeshFilter>();
        resultFilter.sharedMesh = mesh;

        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = _nonWireframeMaterial;

        return resultFilter;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenConsole()
    {
        Reporter.Instance.doShow();
    }
}
