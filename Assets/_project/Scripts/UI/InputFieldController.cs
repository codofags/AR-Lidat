using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int i = 0;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        // Добавляем обработчик события изменения значения в InputField
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        
    }
    private void OnEnable()
    {
        var model = FindObjectOfType<ModelInteraction>();
        if (model != null)
        {
            switch (i)
            {
                case 0:
                    inputField.text = model.RotationSpeed.ToString(); break;
                    case 1:
                    inputField.text = model.ScaleSpeed.ToString(); break;
                    case 2:
                    inputField.text = model.MoveSpeed.ToString(); break;
                    case 3:
                    inputField.text = model.MinFov.ToString(); break;
                    case 4:
                    inputField.text = model.MaxFov.ToString(); break;
            }
        }
    }

    private void OnInputFieldValueChanged(string newText)
    {
        var model = FindObjectOfType<ModelInteraction>();
        if (model == null)
            return;

        if (float.TryParse(newText, out float floatValue))
        {
            model = FindObjectOfType<ModelInteraction>();
            if (i == 0)
            {
                model.RotationSpeed = floatValue;
            }
            else if (i == 1)
            {
                model.ScaleSpeed = floatValue;
            }
            else if (i == 2)
            {
                model.MoveSpeed = floatValue;
            }
            else if (i == 3)
            {
                model.MinFov = floatValue;
            }
            else if (i == 4)
            {
                model.MaxFov = floatValue;
            }
        }
    }
}