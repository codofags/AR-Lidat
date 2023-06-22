using ScanAR.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : Singleton<UIController>
{
    [SerializeField] private MainPanel _mainPanel;
    [SerializeField] private ViewerPanel _viewerPanel;
    [SerializeField] private ExportPanel _exportPanel;

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