using ScanAR.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [field: SerializeField] public TopBar TopBar { get; private set; }
    [field: SerializeField] public MainPanel MainPanel { get; private set; }
    [field: SerializeField] public ViewerPanel ViewerPanel { get; private set; }
    [field: SerializeField] public ExportPanel ExportPanel { get; private set; }
    [field: SerializeField] public ExportStatusPanel ExportStatusPanel { get; private set; }
    [field: SerializeField] public CallPanel CallPanel { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        //TopBar.InfoPanel.Hide();
    }

    public void ShowUI()
    {
        MainPanel.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        MainPanel.gameObject.SetActive(false);
    }

    public void ShowViewerPanel()
    {
        MainPanel.gameObject.SetActive(false );
        ViewerPanel.Show();
    }

    public void HideViewer()
    {
        ViewerPanel.gameObject.SetActive(false);
    }

    public void ShowExportPanel()
    {
        ViewerPanel.gameObject.SetActive(false);
        ExportPanel.Show();
    }
}
