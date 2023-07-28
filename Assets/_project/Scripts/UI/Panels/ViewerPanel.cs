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
        _convertBtn.interactable = false;
        _text.text = "Processing";
        gameObject.SetActive(true);
        _icon.gameObject.SetActive(true);
    }

    private void OnConvertClick()
    {
        //todo Начать скан местности
        
        VRTeleportation_ScanController.Instance.ConvertToModel();
    }

    public void Complete()
    {
        _icon.gameObject.SetActive(false);
        _convertBtn.interactable = true;
        _text.text = "Process";
    }
}