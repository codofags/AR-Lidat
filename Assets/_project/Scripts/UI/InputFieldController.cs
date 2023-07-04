using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int i = 0;

    private void Start()
    {
        // Добавляем обработчик события изменения значения в InputField
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        
    }
    private void OnEnable()
    {
        var model = FindObjectOfType<ThirdPersonCamera>();
        if (model != null)
        {
            switch (i)
            {
                case 0:
                    inputField.text = model.MoveSpeed.ToString(); break;
                    //case 1:
                    //inputField.text = model.ZoomSpeed.ToString(); break;
                    //case 2:
                    //inputField.text = model.MoveSpeed.ToString(); break;
                    //case 3:
                    //inputField.text = model.MinFov.ToString(); break;
                    //case 4:
                    //inputField.text = model.MaxFov.ToString(); break;
            }
        }
    }

    private void OnInputFieldValueChanged(string newText)
    {
        var model = FindObjectOfType<ThirdPersonCamera>();
        if (model == null)
            return;

        if (float.TryParse(newText, out float floatValue))
        {
            model = FindObjectOfType<ThirdPersonCamera>();
            switch (i)
            {
                case 0:
                    model.MoveSpeed = floatValue; break;
                //case 1:
                //    model.ZoomSpeed = floatValue; break;
                    //case 2:
                    //inputField.text = model.MoveSpeed.ToString(); break;
                    //case 3:
                    //inputField.text = model.MinFov.ToString(); break;
                    //case 4:
                    //inputField.text = model.MaxFov.ToString(); break;
            }
        }
    }
}