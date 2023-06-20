using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabelScene : MonoBehaviour
{
    public TMP_Text labelText;

    public void SetText(string text)
    {
        labelText.text = text;
    }
}
