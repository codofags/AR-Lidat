using UnityEngine;
using UnityEngine.UI;

public class ObjectRotator : MonoBehaviour
{
    public Slider rotationSlider; // Ссылка на слайдер в инспекторе
    public float rotationSpeed = 60.0f; // Скорость вращения объекта

    private void Update()
    {
        // Получаем текущее значение слайдера и преобразуем его в угол поворота
        float sliderValue = rotationSlider.value;
        float rotationAngle = sliderValue * 180.0f;

        // Применяем вращение вокруг оси Y
        transform.rotation = Quaternion.Euler(0, rotationAngle, 0) ;
    }
}