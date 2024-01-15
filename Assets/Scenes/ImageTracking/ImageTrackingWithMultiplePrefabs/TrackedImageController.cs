using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class TrackedImageController : MonoBehaviour
{
   public ARTrackedImageManager trackedImageManager;
    public GameObject markerPrefab; // Префаб, который будет использоваться в качестве маркера
    public GameObject targetObject; // Объект, который вы хотите переместить
    public Texture2D targetTexture; // Целевая текстура для маркера

    public void Relt()
    {
        // Подписываемся на события ARTrackedImageManager
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        
        foreach (var trackedImage in eventArgs.added)
        {
         Debug.Log("d");
            // Создаем маркер и устанавливаем целевую текстуру
            //GameObject marker = Instantiate(markerPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            ///marker.GetComponent<Renderer>().material.mainTexture = targetTexture;
        }

        

        foreach (var trackedImage in eventArgs.removed)
        {
            // Удаляем маркер, если требуется
        }
    }
}