using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextHelper : MonoBehaviour
{
    [SerializeField] private Text _textField;

    public void OnSliderValueChanged(float value)
    {
        _textField.text = value.ToString();
    }
}
