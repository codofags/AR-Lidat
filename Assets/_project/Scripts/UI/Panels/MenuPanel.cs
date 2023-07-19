using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button _menuBtn;
    [SerializeField] private TMP_Text _infoText;

    [Space]
    [SerializeField] private Button _newScanBtn;
    [SerializeField] private Button _historyBtn;
    [SerializeField] private Button _documentationBtn;
    [SerializeField] private Button _aboutBtn;
    
    [Space]
    [SerializeField] private Sprite _defailtSprite;
    [SerializeField] private Sprite _backSprite;

    private bool _isShown = false;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _menuBtn.onClick.AddListener(MenuClick);
        _newScanBtn.onClick.AddListener(NewScanClick);
        _historyBtn.onClick.AddListener(MenuClick);
        _documentationBtn.onClick.AddListener(MenuClick);
        _aboutBtn.onClick.AddListener(MenuClick);
    }

    private void NewScanClick()
    {
        ScanController.Instance.NewScan();
    }

    private void OnDestroy()
    {
        _menuBtn.onClick.RemoveAllListeners();
    }

    [ContextMenu("Show")]
    public void Show()
    {
        _rectTransform.transform.DOLocalMoveX(-249.5261f, 0.35f).OnComplete(()=> _isShown = true);
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        _rectTransform.transform.DOLocalMoveX(-553.5261f, 0.35f).OnComplete(() => _isShown = false);
    }

    public void MenuClick()
    {
        if (!_isShown)
        {
            _menuBtn.image.sprite = _backSprite;
            Show();
        }
        else
        {
            _menuBtn.image.sprite = _defailtSprite;
            Hide();
        }
    }    
}
