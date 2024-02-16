using UnityEngine;
using UnityEngine.UI;

public class DistanceMeasurement : MonoBehaviour
{
    public Transform targetObject; // Объект, расстояние до которого мы измеряем
    public Transform cameraTransform; // Камера
    public GameObject objectToRight;
    public GameObject objectToDown;
    public GameObject button;
    public float rotationThreshold = 0.5f;
    private bool wasRotatingRight = false;
    private bool wasRotatingDown = false;
    public Text distanceText; // Текстовое поле для вывода расстояния
    bool see;
    public Color remove1;
    public Color remove2;
    public Material material;
    public float movementThreshold = 1.5f;
    private void Update()
    {
        UpdateL();

        if (see == true)
        {
            // Проверяем, что оба объекта существуют
            if (targetObject != null && cameraTransform != null)
            {
                // Измеряем расстояние между объектом и камерой
                float distance = Vector3.Distance(targetObject.position, cameraTransform.position);
                if (distance >= 598.98)
                {

                    // Выводим расстояние в текстовое поле
                    distanceText.text = "Теперь можно зафиксировать объект";
                    material.SetColor("_ReplacementColor", remove1);
                    button.SetActive(true);

                }
                else
                {
                    distanceText.text = "Отойдите дальше от объекта";
                    material.SetColor("_ReplacementColor", remove2);
                }
            }
            
        }
    }


    private void UpdateL()
    {

        // Получаем данные акселерометра
        Vector3 accelerometerData = Input.acceleration;

        // Проверяем движение телефона вправо
        if (accelerometerData.x > movementThreshold)
        {
            if (objectToRight != null && objectToRight.activeSelf)
            {
                objectToRight.SetActive(false);
                see = true;
            }
        }
        
    }

}