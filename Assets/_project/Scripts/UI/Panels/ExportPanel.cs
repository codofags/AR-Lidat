using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExportPanel : MonoBehaviour
{
    [SerializeField] private Button _exportBtn;
    [SerializeField] private TMP_Text _exportText;
    [SerializeField] private GameObject _exportPanel;
    [SerializeField] private Image _icon;
    [SerializeField] private TMPro.TMP_InputField _nameInput;

    private void Awake()
    {
        _exportBtn.onClick.AddListener(ExportClick);
    }

    private void OnDestroy()
    {
        _exportBtn.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        _exportBtn.interactable = false;
        _icon.gameObject.SetActive(true);
        _exportText.text = "Обработка";
        gameObject.SetActive(true);
    }

    private void ExportClick()
    {
        _exportPanel.SetActive(true);
    }

    public void OnExportClick()
    {
        ScanController.Instance.ExportModel(_nameInput.text);
    }

    public void Complete()
    {
        _icon.gameObject.SetActive(false);
        _exportBtn.interactable = true;
        _exportText.text = "Обработать";
    }
}
