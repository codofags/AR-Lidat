using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Button _menuBtn;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private string _documentationURL;

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
        _historyBtn.onClick.AddListener(HistoryClick);
        _documentationBtn.onClick.AddListener(DocumentationClick);
        _aboutBtn.onClick.AddListener(MenuClick);
    }

    private void DocumentationClick()
    {
        // Проверяем, что URL не пустой
        if (!string.IsNullOrEmpty(_documentationURL))
        {
            // Открываем гиперссылку
            Application.OpenURL(_documentationURL);
        }
        else
        {
            Debug.LogWarning("URL is empty or null.");
        }
    }

    private void HistoryClick()
    {
        VRTeleportation_NetworkBehviour.Instance.SendGetModelList();
        MenuClick();
    }

    private void NewScanClick()
    {
        VRTeleportation_ScanController.Instance.Restart();
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
