using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public Dictionary<int, ScanData> SavedCameraData = new Dictionary<int, ScanData>();
    private int _currentId = 0;

    private void Start()
    {
        TextureGetter.Instance.Initialize(OnTextureGetted);
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
            yield return new WaitForSeconds(1f);
            //CheckCameraForSave();
            SavedCameraData.Add(_currentId, new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
            TextureGetter.Instance.GetImageAsync(_currentId);

            ++_currentId;
        }
    }

    private void CheckCameraForSave()
    {
        float difference = float.MaxValue;
        foreach (var camData in SavedCameraData)
        {
            // ѕровер€ем разницу в повороте и позиции камеры
            float rotationDifference = Quaternion.Angle(camData.Value.Rotation, transform.rotation);
            float positionDifference = Vector3.Distance(camData.Value.Position, transform.position);

            var dif = rotationDifference + positionDifference;

            if (difference > dif)
            {
                difference = dif;
            }
        }

        if (difference > .5f)
        {
            SavedCameraData.Add(_currentId, new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
            TextureGetter.Instance.GetImageAsync(_currentId);

            ++_currentId;
        }
        Debug.Log($"Cam Dif: {difference}");
    }

    private void OnTextureGetted(Texture2D texture, int id)
    {
        SavedCameraData[id].Texture = texture;
    }

}

public class ScanData
{
    public int Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public Texture2D Texture;
}
