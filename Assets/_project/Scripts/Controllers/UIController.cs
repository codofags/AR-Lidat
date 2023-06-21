using ScanAR.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : Singleton<UIController>
{
    [SerializeField] private MainPanel _mainPanel;

    public void ShowUI()
    {
        _mainPanel.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        _mainPanel.gameObject.SetActive(false);
    }
}
