using UnityEngine;

public class ExportPanel : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField _nameInput;

    public void OnExportClick()
    {
        ScanController.Instance.ExportModel(_nameInput.text);
    }
}
