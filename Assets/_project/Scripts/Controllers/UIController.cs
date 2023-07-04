using ScanAR.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : Singleton<UIController>
{
    [field: SerializeField] public InfoPanel InfoPanel { get; private set; }
    [SerializeField] private MainPanel _mainPanel;
    [SerializeField] private ViewerPanel _viewerPanel;
    [SerializeField] private ExportPanel _exportPanel;

    protected override void Awake()
    {
        base.Awake();
        InfoPanel.Hide();
    }

    public void ShowUI()
    {
        _mainPanel.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        _mainPanel.gameObject.SetActive(false);
    }

    public void ShowViewerPanel()
    {
        _mainPanel.gameObject.SetActive(false );
        _viewerPanel.gameObject.SetActive(true);
    }

    public void ShowExportPanel()
    {
        _viewerPanel.gameObject.SetActive(false);
        _exportPanel.gameObject.SetActive(true);
    }
}