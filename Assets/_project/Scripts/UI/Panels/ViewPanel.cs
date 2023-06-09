using UnityEngine;
using UnityEngine.UI;

public class ViewPanel : MonoBehaviour
{
    [SerializeField] private Button _convertBtn;

    private void Awake()
    {
        _convertBtn.onClick.AddListener(OnConvertBtnClick);
    }
    private void OnDestroy()
    {
        _convertBtn.onClick.RemoveAllListeners();
    }
    private void OnConvertBtnClick()
    {
        //todo Начать наложение текстур на меш
    }
}