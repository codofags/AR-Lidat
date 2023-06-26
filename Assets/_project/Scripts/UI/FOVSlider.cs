using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class FOVSlider : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    
    private Slider _fovSlider;

    private void Awake()
    {
        _fovSlider = GetComponent<Slider>();
    }

    void Start()
    {
        // Установка начального значения слайдера равным текущему FOV камеры
        _fovSlider.value = _arCamera.fieldOfView;

        // Добавление обработчика события изменения значения слайдера
        _fovSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        // Изменение поля зрения камеры на основе значения слайдера
        _arCamera.fieldOfView = value;
    }
}

