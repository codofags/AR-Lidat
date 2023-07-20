using TMPro;
using UnityEngine;

public class TopBar : MonoBehaviour
{
    [SerializeField] private TMP_Text _infoText;
    [field: SerializeField] public InfoPanel InfoPanel { get; private set; }

    public void SetInfoText(string text)
    {
        _infoText.SetText(text);
    }
}
