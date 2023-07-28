using HoloGroup.Networking.Internal.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class VRTeleportation_ScanController : Singleton<VRTeleportation_ScanController>
{
    private string SCAN_TEXT = "Scanning";
    private string MESH_CONVERT_START_TEXT = "Creating Mesh\r\nStatus: Processing";
    private string MESH_CONVERT_END_TEXT = "Viewing the Mesh";
    private string MESH_TEXTURE_START_TEXT = "Texture mapping\r\nStatus: Processing";
    private string MESH_TEXTURE_END_TEXT = "Viewing the model";


    [SerializeField] private Camera _checkMeshCamera;
    [SerializeField] private MeshSlicer _slicer;
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _cameraViewer;
    [SerializeField] private Transform _modelViewParent;

    [SerializeField] private Transform _cameraViewPrefab;
    [SerializeField] private Material _nonWireframeMaterial;

    private bool _isScanning = false;

    [SerializeField] private MeshFilter[] TestMeshes;

    private Vector2 _uvOffset = Vector2.zero;
    private List<GameObject> _slicedMeshes = new List<GameObject>();
    private List<Transform> _ghostCameras = new List<Transform>();
    private bool isExporting = false;

    protected override void Awake()
    {
        base.Awake();
        UIController.Instance.TopBar.SetInfoText(SCAN_TEXT);
        _isScanning = false;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;

        if (_arMeshManager.meshes != null && _arMeshManager.meshes.Count > 0)
            _arMeshManager.meshes.Clear();
    }

    private void OnEnable()
    {
        //_arMeshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnDisable()
    {
        //_arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopAllCoroutines();
    }

    public void ScanStart()
    {
        if (!_isScanning)
        {
            UIController.Instance.TopBar.SetInfoText(SCAN_TEXT);
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
            _isScanning = true;

            StartCoroutine(Scaning());
        }
    }

    IEnumerator Scaning()
    {
        yield return new WaitForSeconds(_scanningTime);

        if (!_isScanning)
            yield break;

        ScanStop();
    }

    [ContextMenu("StopScan")]
    public void ScanStop()
    {
        if (_isScanning)
        {
            if (_arCameraManager != null)
            {
                if (_arCameraManager.TryGetComponent(out Camera cam))
                {
                    _checkMeshCamera.fieldOfView = cam.fieldOfView;
                }
            }

            CameraPositionSaver.Instance.StopSaving();

            if (_arMeshManager != null)
            {
                _arMeshManager.enabled = false; // Отключаем ARMeshManager

                XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

                if (arMeshSubsystem != null)
                {
                    arMeshSubsystem.Stop();
                }
            }

            _isScanning = false;
            _cameraViewer.gameObject.SetActive(true);
            UIController.Instance.ShowViewerPanel();
            //UIController.Instance.Fade.enabled = true;
            UIController.Instance.TopBar.SetInfoText(MESH_CONVERT_START_TEXT);
            

            StartCoroutine(Stopping());
        }
    }

    IEnumerator Stopping()
    {
        var topBar = UIController.Instance.TopBar;
        int steps = 6;
        int tempStep = 0;
        Debug.Log(_arMeshManager == null);
        topBar.InfoPanel.Show();
        topBar.InfoPanel.Process((tempStep * 100) / steps);
        yield return new WaitForSeconds(1);

        Debug.Log("step 1");
        tempStep++;
        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.transform.SetParent(_modelViewParent, false);

            var renderer = meshFilter.GetComponent<MeshRenderer>();
            renderer.material = _nonWireframeMaterial;
            renderer.material.color = UnityEngine.Random.ColorHSV();
        }

        topBar.InfoPanel.Process((tempStep * 100) / steps);
        yield return new WaitForEndOfFrame();
        Debug.Log("step 2");
        tempStep++;
        var cameraDatas = CameraPositionSaver.Instance.SavedCameraData;
        _checkMeshCamera.transform.parent = _modelViewParent;

        topBar.InfoPanel.Process((tempStep * 100) / steps);
        yield return new WaitForEndOfFrame();
        Debug.Log("step 3");
        tempStep++;
        _ghostCameras.Clear();
        foreach (var camPos in cameraDatas)
        {
            if (camPos.Texture == null)
                continue;

            var newCameraView = Instantiate(_cameraViewPrefab, _modelViewParent);
            newCameraView.localPosition = camPos.Position;
            newCameraView.localRotation = camPos.Rotation;
            newCameraView.localScale = Vector3.one * 0.1f;
            _ghostCameras.Add(newCameraView);
        }

        topBar.InfoPanel.Process((tempStep * 100) / steps);
        yield return new WaitForSeconds(1f);

        Debug.Log("step 4");
        tempStep++;
        var combinedObject = CombineMeshes(_arMeshManager.meshes);
        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.gameObject.SetActive(false);
        }

        _arCameraManager.enabled = false;

        topBar.InfoPanel.Process((tempStep * 100) / steps);
        Debug.Log("WAIT 5 sec");
        yield return new WaitForSeconds(5f);

        Debug.Log("step 5");
        tempStep++;
        topBar.InfoPanel.Process((tempStep * 100) / steps);
        yield return new WaitForEndOfFrame();
        _slicedMeshes = _slicer.SliceMesh(combinedObject, _nonWireframeMaterial);
        Debug.Log($"Mesh count: {_slicedMeshes.Count}");

        foreach (var sMesh in _slicedMeshes)
        {
            sMesh.transform.SetParent(_modelViewParent, false);
        }

        topBar.InfoPanel.Process(100);
        //_initPos = mesh.transform.position;
        //_initRot = mesh.transform.rotation;
        UIController.Instance.ShowViewerPanel();
        topBar.InfoPanel.Hide();
        topBar.SetInfoText(MESH_CONVERT_END_TEXT);
        UIController.Instance.ViewerPanel.Complete();
        var model = FindObjectOfType<ThirdPersonCamera>();

        if (model != null)
            model.IsInteractable = true;

        Debug.Log($"Meshes Load: {_slicedMeshes.Count}. DONE.");
    }

    public void ConvertToModel()
    {
        //UIController.Instance.Fade.enabled = true;
        UIController.Instance.TopBar.SetInfoText(MESH_TEXTURE_START_TEXT);
        UIController.Instance.HideViewer();
        UIController.Instance.ShowExportPanel();
        StartCoroutine(Converting());
    }

    IEnumerator Converting()
    {
        yield return null;
#if !UNITY_EDITOR
        var model = FindObjectOfType<ThirdPersonCamera>();
        if (model != null)
            model.IsInteractable = false;

        yield return null;
        var cameraDatas = CameraPositionSaver.Instance.SavedCameraData;
        float steps = cameraDatas.Count;
        float tempStep = 0;
        var infoPanel = UIController.Instance.TopBar.InfoPanel;
        infoPanel.Show();
        infoPanel.Process((tempStep * 100f) / steps);
        Debug.Log("WAIT 10 sec");
        yield return new WaitForSeconds(10f);

        var handledMeshes = new List<GameObject>();

        foreach (var camData in cameraDatas)
        {
            infoPanel.Process((tempStep * 100f) / steps);
            tempStep++;
            if (_slicedMeshes.Count == 0)
                break;

            int handledCount = 0;
            for (int i = 0; i < _slicedMeshes.Count; ++i)
            {
                if (_slicedMeshes[i].name.StartsWith("Handled"))
                {
                    continue;
                    //var mf = _slicedMeshes[i].GetComponent<MeshFilter>();
                    //_checkMeshCamera.transform.localPosition = camData.Position;
                    //_checkMeshCamera.transform.localRotation = camData.Rotation;
                    //if (_checkMeshCamera.IsMeshFullyIn(mf))
                    //{
                    //    var uv2 = mf.GetGeneratedUV(_checkMeshCamera, camData.Texture, _uvOffset);
                    //    var render = mf.GetComponent<MeshRenderer>();
                    //    mf.CombineTextures(camData.Texture, uv2);
                    //    //render.material = _nonWireframeMaterial;
                    //    //render.material.color = Color.white;
                    //    //render.material.SetTexture("_BaseMap", camData.Texture);

                    //    mf.name = $"Handled_{mf.name}";
                    //    ++handledCount;
                    //    handledMeshes.Add(mf.gameObject);
                    //}
                }
                else
                {
                    var mf = _slicedMeshes[i].GetComponent<MeshFilter>();
                    _checkMeshCamera.transform.localPosition = camData.Position;
                    _checkMeshCamera.transform.localRotation = camData.Rotation;
                    if (_checkMeshCamera.IsMeshFullyIn(mf))
                    {
                        mf.GenerateUV(_checkMeshCamera, camData.Texture, _uvOffset);
                        var render = mf.GetComponent<MeshRenderer>();
                        render.material = _nonWireframeMaterial;
                        render.material.color = Color.white;
                        render.material.SetTexture("_BaseMap", camData.Texture);

                        mf.name = $"Handled_{mf.name}";
                        ++handledCount;
                        handledMeshes.Add(mf.gameObject);
                    }
                }                
            }
            infoPanel.Process((tempStep * 100f) / steps);
            yield return new WaitForEndOfFrame();
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

        infoPanel.Hide();
#endif
        UIController.Instance.TopBar.SetInfoText(MESH_TEXTURE_END_TEXT);
        UIController.Instance.ExportPanel.Complete();

        Debug.Log("DONE Converting");
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
        // Устанавливаем формат индекса Unit32
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
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
        //ARSession session = FindObjectOfType<ARSession>();

        //if (session != null)
        //{
        //    session.Reset();
        //    session.enabled = true;
        //}

        ////if (_arMeshManager.meshes.Count > 0)
        ////{
        ////    _arMeshManager.meshes.Clear();
        ////}

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public async void ExportModel(string name)
    {
        if (string.IsNullOrEmpty(name))
            name = "NONAME";

        if (isExporting)
        {
            Debug.Log("Export is already in progress.");
            return;
        }

        UIController.Instance.TopBar.InfoPanel.Show();
        isExporting = true;

        _ghostCameras.ForEach(cam => cam.gameObject.SetActive(false));
        var serializer = new ModelSerializer();
        var modelData = serializer.Serialize(_modelViewParent.gameObject);

        await VRTeleportation_NetworkBehviour.Instance.SendModel(modelData, name, (percent) =>
        {
            UIController.Instance.TopBar.InfoPanel.Process(percent);
            Debug.Log($"Percent: {percent} %");
        });

        isExporting = false;
        Debug.Log("Model exported successfully.");
    }

    public void NewScan()
    {
        ARSession session = FindObjectOfType<ARSession>();
        session.enabled = false;

        // Остановка ARMeshManager
        _arMeshManager.enabled = false;

        // Очистка существующего меша
        if (_arMeshManager.meshes.Count > 0)
        {
            foreach (var mesh in _arMeshManager.meshes)
            {
                Destroy(mesh.gameObject);
            }
            _arMeshManager.meshes.Clear();
        }

        session.enabled = true;
        // Включение ARMeshManager для нового сканирования
        _arMeshManager.enabled = true;
        Restart();
    }

    public void OpenConsole()
    {
        Reporter.Instance.doShow();
    }

    public void OnOffsetChangedU(float u)
    {
        _uvOffset.x = u;
    }

    public void OnOffsetChangedV(float v)
    {
        _uvOffset.y = v;
    }
}
