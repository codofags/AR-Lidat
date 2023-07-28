using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExportStatusPanel : MonoBehaviour
{
    [SerializeField] private GameObject _succesPanel;
    [SerializeField] private GameObject _errorPanel;
    [SerializeField] private Button _againBtn;

    public void Show(string modelName, bool succes = true)
    {
        _againBtn.onClick.RemoveAllListeners();
        _againBtn.onClick.AddListener(() => Again(modelName));
        gameObject.SetActive(true);
        _succesPanel.SetActive(succes);
        _errorPanel.SetActive(!succes);
        UIController.Instance.TopBar.InfoPanel.Hide();
    }

    private void Again(string modelName)
    {
        Hide();
        VRTeleportation_ScanController.Instance.ExportModel(modelName);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
