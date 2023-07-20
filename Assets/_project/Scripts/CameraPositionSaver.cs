using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPositionSaver : Singleton<CameraPositionSaver>
{
    [SerializeField] private Image _testImage;
    [SerializeField] private Image _testImageRaw;

    private Coroutine _savingProcesss;
    private Coroutine _getCameraTextureProcess;

    public List<ScanData> SavedCameraData = new List<ScanData>();
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
            //yield return new WaitForSeconds(.5f);
            yield return new WaitForEndOfFrame();
            CheckCameraForSave();
            //SavedCameraData.Add(new ScanData() { Id = _currentId, Position = transform.position, Rotation = transform.rotation });
            //TextureGetter.Instance.GetImageAsync(_currentId);

            //++_currentId;
        }
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
            TextureGetter.Instance.GetImageAsync(_currentId);

            _currentId++;
        }
        else
        {
            //Debug.Log("Camera data not saved.");
        }
    }

    private void OnTextureGetted(Texture2D texture, int id)
    {
        //SavedCameraData[id].Texture = texture;
        //Debug.Log($"Cameras: {SavedCameraData.Count}");

        var rawSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        _testImageRaw.sprite = rawSprite;

        var offset = (int)((texture.width / 2) - 333);
        Debug.Log($"Texture: {texture.width}/{texture.height}  offset: {offset}");

        var cuttedColors = texture.GetPixels(offset, 0, 666, 1440, 0);
        var cuttedTexture = new Texture2D(666, 1440, texture.format, false);

        cuttedTexture.SetPixels(cuttedColors);
        cuttedTexture.Apply();

        var asSprite = Sprite.Create(cuttedTexture, new Rect(0, 0, cuttedTexture.width, cuttedTexture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        _testImage.sprite = asSprite;

        SavedCameraData[id].Texture = cuttedTexture;
        Debug.Log($"Cameras: {SavedCameraData.Count}");
    }

}

public class ScanData
{
    public int Id;
    public Vector3 Position;
    public Quaternion Rotation;
    public Texture2D Texture;
}
