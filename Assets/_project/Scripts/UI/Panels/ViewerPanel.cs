using UnityEngine;
using UnityEngine.UI;

public class ViewerPanel : MonoBehaviour
{
    [SerializeField] private Button _convertBtn;

    private void Awake()
    {
        _convertBtn.onClick.AddListener(OnConvertClick);
    }
    private void OnDestroy()
    {
        _convertBtn.onClick.RemoveAllListeners();
    }

    private void OnConvertClick()
    {
        //todo Начать скан местности
        ScanController.Instance.ConvertToModel();
        UIController.Instance.ShowExportPanel();
    }
}
