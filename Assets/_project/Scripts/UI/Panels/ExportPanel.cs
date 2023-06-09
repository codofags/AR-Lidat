using Autodesk.Fbx;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ExportPanel : MonoBehaviour
{
    [SerializeField] private Button _exportBtn;

    private void Awake()
    {
        _exportBtn.onClick.AddListener(TaskOnClick);
    }

    private void OnDestroy()
    {
        _exportBtn.onClick.RemoveAllListeners();
    }

    void TaskOnClick()
    {
        var exporter = FindObjectOfType<RuntimeFBXExporter>();
        exporter.ExportToFBX();
    }
}
