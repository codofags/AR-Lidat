using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Show(string text)
    {
        SetText(text);
        gameObject.SetActive(true);
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}