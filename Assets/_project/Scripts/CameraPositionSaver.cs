using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UIElements;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public Dictionary<int, ScanData> SavedCameraData = new Dictionary<int, ScanData>();
    private int _currentId = 0;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

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
        SavedCameraData.Add(_currentId, new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
        TextureGetter.Instance.GetImageAsync(_currentId);

        _currentId++;
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
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
            //yield return new WaitForSeconds(1f);
            yield return new WaitForEndOfFrame();
            CheckCameraForSave();
            //SavedCameraData.Add(_currentId, new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
            //TextureGetter.Instance.GetImageAsync(_currentId);

            //++_currentId;
        }
    }

    private void CheckCameraForSave()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // ѕровер€ем изменение позиции и поворота камеры
        float positionDifference = Vector3.Distance(_lastPosition, currentPosition);
        float rotationDifference = Quaternion.Angle(_lastRotation, currentRotation);

        if (positionDifference >= 5f || rotationDifference >= 45f)
        {
            Debug.Log($"Camera Save: {transform.position} - {transform.rotation}");
            SavedCameraData.Add(_currentId, new ScanData() { Id = _currentId, Position = currentPosition, Rotation = currentRotation });
            TextureGetter.Instance.GetImageAsync(_currentId);

            _currentId++;
            _lastPosition = currentPosition;
            _lastRotation = currentRotation;
        }

        Debug.Log($"Cam Dif: Position={positionDifference}, Rotation={rotationDifference}");
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
