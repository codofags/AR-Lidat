using ScanAR.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [field: SerializeField] public TopBar TopBar { get; private set; }
    [SerializeField] private MainPanel _mainPanel;
    [field: SerializeField] public ViewerPanel ViewerPanel { get; private set; }
    [SerializeField] private ExportPanel _exportPanel;
    [field: SerializeField] public Image Fade { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        TopBar.InfoPanel.Hide();
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
        ViewerPanel.Show();
    }

    public void HideViewer()
    {
        ViewerPanel.gameObject.SetActive(false);
    }

    public void ShowExportPanel()
    {
        ViewerPanel.gameObject.SetActive(false);
        _exportPanel.gameObject.SetActive(true);
    }
}