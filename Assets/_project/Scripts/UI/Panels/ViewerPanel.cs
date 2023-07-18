using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewerPanel : MonoBehaviour
{
    [SerializeField] private Button _convertBtn;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _text;

    private void Awake()
    {
        _convertBtn.onClick.AddListener(OnConvertClick);
    }

    private void OnDestroy()
    {
        _convertBtn.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _icon.enabled = true;
    }

    private void OnConvertClick()
    {
        //todo Начать скан местности
        
        ScanController.Instance.ConvertToModel();
    }

    public void Complete()
    {
        _icon.enabled = false;

        if(_text != null)
            _text.text = "Обработать";
    }
}