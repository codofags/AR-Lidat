using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    [SerializeField] private int _textureLowingDevider = 2;
    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public List<ScanData> SavedCameraData = new List<ScanData>();

    [HideInInspector]public Texture2D FrameTexture;
    private int _currentId = 0;

    private void Start()
    {
        VRTeleportation_TextureGetter.Instance.Initialize(OnTextureGetted, _textureLowingDevider);

        VRTeleportation_TextureGetter.Instance.Initialize(OnTextureGet);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void StartSaving()
    {
        Debug.Log("StartSaving");
        _savingProcesss = StartCoroutine(SavePositionProcess());
        //_getCameraTextureProcess = StartCoroutine(SaveTextureProcess());
    }

    public void StartSavingOneFrame()
    {
        Debug.Log("StartSavingOneFrame");
        _savingProcesss = StartCoroutine(SaveProcess());
        //_getCameraTextureProcess = StartCoroutine(SaveTextureProcess());
    }

    public void StopSaving()
    {
        if (_savingProcesss != null)
            StopCoroutine(_savingProcesss);

        if (_getCameraTextureProcess != null)
            StopCoroutine(_getCameraTextureProcess);
        Debug.Log("StopSaving");
    }


    private IEnumerator SavePositionProcess()
    {
        while(true)
        {
            //yield return new WaitForSeconds(.5f);
            yield return new WaitForEndOfFrame();
            CheckCameraForSave();
            //SavedCameraData.Add(new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
            //TextureGetter.Instance.GetImageAsync(_currentId);

            //++_currentId;
        }
    }

    private IEnumerator SaveProcess()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            FrameCameraSave();
        }
    }

    private void FrameCameraSave()
    {
           VRTeleportation_TextureGetter.Instance.GetImageAsync();
    }

    private void CheckCameraForSave()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        bool shouldSave = true;

        foreach (var savedCameraData in SavedCameraData)
        {
            float positionDifference = Vector3.Distance(savedCameraData.Position, currentPosition);
            float rotationDifference = Quaternion.Angle(savedCameraData.Rotation, currentRotation);

            if (positionDifference < .5f && rotationDifference < 15f)
            {
                shouldSave = false;
                break; // Если хотя бы с одной камеры условие выполняется, выходим из цикла и не сохраняем новые данные
            }
        }

        if (shouldSave)
        {
            Debug.Log($"Camera Save: {transform.position} - {transform.rotation}");
            SavedCameraData.Add(new ScanData() { Id = _currentId, Position = currentPosition, Rotation = currentRotation });
            VRTeleportation_TextureGetter.Instance.GetImageAsync(_currentId);

            _currentId++;
        }
        else
        {
            //Debug.Log("Camera data not saved.");
        }
    }

    private void OnTextureGetted(Texture2D texture, int id)
    {
        var offset = (int)((texture.width - 888 / _textureLowingDevider) / 2);

        var cuttedColors = texture.GetPixels(offset, 0, 888 / _textureLowingDevider, 1920 / _textureLowingDevider, 0);
        var cuttedTexture = new Texture2D(888/ _textureLowingDevider, 1920/ _textureLowingDevider, texture.format, false);

        cuttedTexture.SetPixels(cuttedColors);
        cuttedTexture.Apply();

        SavedCameraData[id].Texture = cuttedTexture;
        Debug.Log($"Cameras: {SavedCameraData.Count}");
    }

    private void OnTextureGet(Texture2D texture)
    {
        var offset = (int)((texture.width - 888 / _textureLowingDevider) / 2);

        var cuttedColors = texture.GetPixels(offset, 0, 888 / _textureLowingDevider, 1920 / _textureLowingDevider, 0);
        var cuttedTexture = new Texture2D(888 / _textureLowingDevider, 1920 / _textureLowingDevider, texture.format, false);

        cuttedTexture.SetPixels(cuttedColors);
        cuttedTexture.Apply();

        FrameTexture = cuttedTexture;
    }

}

public class ScanData
{
    public int Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public Texture2D Texture;
}
