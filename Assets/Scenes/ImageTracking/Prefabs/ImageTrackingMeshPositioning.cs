using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ImageTrackingMeshPositioning : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager trackedImageManager;
    [SerializeField] GameObject meshObject; // Меш, который вы хотите позиционировать
    public Text txt;
    private void OnEnable()
    {
        // Подпишемся на события Image Tracked
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        // Отпишемся от событий Image Tracked
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Проверим, является ли это изображение интересующим нас
            //if (trackedImage.referenceImage.name == "one")
            {
                // Получим позицию и поворот изображения
                Vector3 position = trackedImage.transform.position;
                Quaternion rotation = trackedImage.transform.rotation;
                txt.text=trackedImage.referenceImage.name;

                // Позиционируем меш
                meshObject.transform.position = position;
                //meshObject.transform.rotation = rotation;

                // Активируем меш (если он не активен)
                meshObject.SetActive(true);
            }
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            // Если изображение обновилось, то выполните аналогичные действия
            //if (trackedImage.referenceImage.name == "one")
            {
                Vector3 position = trackedImage.transform.position;
                Quaternion rotation = trackedImage.transform.rotation;

                meshObject.transform.position = position;
                //meshObject.transform.rotation = rotation;
            }
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            // Если изображение больше не распознается, можно скрыть или деактивировать меш
            ///meshObject.SetActive(false);
        }
    }
}
